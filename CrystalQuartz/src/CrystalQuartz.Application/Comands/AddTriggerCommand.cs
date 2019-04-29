using System;
using CrystalQuartz.Application.Comands.Inputs;
using CrystalQuartz.Application.Comands.Outputs;
using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.Domain.TriggerTypes;

namespace CrystalQuartz.Application.Comands
{
    using System.Collections.Generic;
    using System.Linq;
    using CrystalQuartz.Core.Domain.ObjectInput;

    public class AddTriggerCommand : AbstractSchedulerCommand<AddTriggerInput, AddTriggerOutput>
    {
        private readonly RegisteredInputType[] _registeredInputTypes;

        public AddTriggerCommand(
            Func<SchedulerHost> schedulerHostProvider,
            RegisteredInputType[] registeredInputTypes) : base(schedulerHostProvider)
        {
            _registeredInputTypes = registeredInputTypes;
        }

        protected override void InternalExecute(AddTriggerInput input, AddTriggerOutput output)
        {
            IDictionary<string, object> jobDataMap = null;

            if (input.JobDataMap != null)
            {
                jobDataMap = new Dictionary<string, object>();

                IDictionary<string, string> validationErrors = new Dictionary<string, string>();

                foreach (JobDataItem item in input.JobDataMap)
                {
                    //dataMap  input类型
                    RegisteredInputType inputType = _registeredInputTypes.FirstOrDefault(x => x.InputType.Code == item.InputTypeCode);
                    if (inputType == null)
                    {
                        /*
                         *只有当客户端输入类型定义与服务器端不同步时，我们才能到达这里。
                         */
                        validationErrors[item.Key] = "不知道的 input type: " + item.InputTypeCode;
                    }
                    else
                    {
                        try
                        {
                            var value = inputType.Converter == null
                                ? item.Value
                                : inputType.Converter.Convert(item.Value);

                            jobDataMap[item.Key] = value;
                        }
                        catch (Exception ex)
                        {
                            validationErrors[item.Key] = ex.Message;
                        }
                    }
                }

                if (validationErrors.Any())
                {
                    output.ValidationErrors = validationErrors;

                    return;
                }
            }

            if (!string.IsNullOrEmpty(input.JobClass))
            {
                //反射Class
                Type jobType = Type.GetType(input.JobClass, true);

                //if (!SchedulerHost.AllowedJobTypesRegistry.List().Contains(jobType))
                //{
                //    output.Success = false;
                //    output.ErrorMessage = "Job type " + jobType.FullName + " 是不允许的";
                //    return;
                //}

                SchedulerHost.Commander.ScheduleJob(
                    NullIfEmpty(input.Job),
                    NullIfEmpty(input.Group),
                    jobType,
                    input.Name,
                    CreateTriggerType(input),
                    jobDataMap);
            }
            else
            {
                if (string.IsNullOrEmpty(input.Job))
                {
                    output.Success = false;
                    output.ErrorMessage = "当给一个存在的job 添加触发器时 ,job Name 是必须有值的.";
                }
                else
                {
                    SchedulerHost.Commander.ScheduleJob(
                        input.Job,
                        input.Group,
                        input.Name,
                        CreateTriggerType(input),
                        jobDataMap);
                }
            }
        }

        private static string NullIfEmpty(string value)
        {
            if (value == string.Empty)
            {
                return null;
            }

            return value;
        }

        private static TriggerType CreateTriggerType(AddTriggerInput input)
        {
            switch (input.TriggerType)
            {
                case "Simple":
                   var simpleTriggerType= new SimpleTriggerType(input.RepeatForever ? -1 : input.RepeatCount, input.RepeatInterval, 0 /* todo */);
                    return simpleTriggerType;
                case "Cron":
                    var cronTriggerType = new CronTriggerType(input.CronExpression);
                    return cronTriggerType;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}