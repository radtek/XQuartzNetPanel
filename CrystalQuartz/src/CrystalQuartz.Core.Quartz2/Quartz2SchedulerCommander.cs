namespace CrystalQuartz.Core.Quartz2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CrystalQuartz.Core.Contracts;
    using CrystalQuartz.Core.Domain.TriggerTypes;

    using Quartz;
    using Quartz.Impl.Matchers;

    internal class Quartz2SchedulerCommander : ISchedulerCommander
    {
        private readonly IScheduler _scheduler;

        public Quartz2SchedulerCommander(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        /// <summary>
        /// 给现有的job 创建一个触发器
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="triggerName"></param>
        /// <param name="trigger"></param>
        /// <param name="jobData"></param>
        public void ScheduleJob(string jobName, string jobGroup, string triggerName, TriggerType trigger, IDictionary<string, object> jobData)
        {
            TriggerBuilder triggerBuilder = ApplyTriggerData(
                triggerName,
                trigger,
                TriggerBuilder.Create().ForJob(jobName, jobGroup));

            if (jobData != null)
            {
                triggerBuilder = triggerBuilder.UsingJobData(new JobDataMap(jobData));
            }
            //将给定的trigger与由trigger的设置标识的Job一起调度。
            _scheduler.ScheduleJob(triggerBuilder.Build());
        }

        /// <summary>
        /// 添加一个job 并添加一个触发器
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobType"></param>
        /// <param name="triggerName"></param>
        /// <param name="triggerType"></param>
        /// <param name="jobData"></param>
        public void ScheduleJob(string jobName, string jobGroup, Type jobType, string triggerName, TriggerType triggerType, IDictionary<string, object> jobData)
        {
            var jobBuilder = JobBuilder.Create(jobType);

            if (!string.IsNullOrEmpty(jobName) || !string.IsNullOrEmpty(jobGroup))
            {
                jobBuilder = jobBuilder.WithIdentity(jobName ?? Guid.NewGuid().ToString(), jobGroup);
            }

            if (jobData != null)
            {
                jobBuilder = jobBuilder.UsingJobData(new JobDataMap(jobData));
            }

            TriggerBuilder triggerBuilder = ApplyTriggerData(triggerName, triggerType, TriggerBuilder.Create());

            //将给定的jobdetail添加到调度程序中，并将给定的trigger与之关联。

            //job 持久: jobBuilder.StoreDurably().Build()
            //立即执行: triggerBuilder.StartNow().Build()

            _scheduler.ScheduleJob(jobBuilder.StoreDurably().Build(), triggerBuilder.Build());
        }

        public void DeleteJobGroup(string jobGroup)
        {
            var keys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGroup));
            _scheduler.DeleteJobs(keys.ToList());
        }

        public void DeleteJob(string jobName, string jobGroup)
        {
            _scheduler.DeleteJob(new JobKey(jobName, jobGroup));
        }

        /// <summary>
        /// 从调度程序中取出指示的trigger。
        /// 如果相关job没有任何其他触发器，
        /// 且该job不持久，则该job也将被删除。
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="triggerGroup"></param>
        public void DeleteTrigger(string triggerName, string triggerGroup)
        {
            _scheduler.UnscheduleJob(new TriggerKey(triggerName, triggerGroup));
        }

        public void ExecuteNow(string jobName, string jobGroup)
        {
            _scheduler.TriggerJob(new JobKey(jobName, jobGroup));
        }

        public void PauseAllJobs()
        {
            _scheduler.PauseAll();
        }

        public void PauseJobGroup(string jobGroup)
        {
            _scheduler.PauseJobs(GroupMatcher<JobKey>.GroupEquals(jobGroup));
        }

        public void PauseJob(string jobName, string jobGroup)
        {
            _scheduler.PauseJob(new JobKey(jobName, jobGroup));
        }

        public void PauseTrigger(string triggerName, string triggerGroup)
        {
            var triggerKey = new TriggerKey(triggerName, triggerGroup);
            _scheduler.PauseTrigger(triggerKey);
        }

        public void ResumeAllJobs()
        {
            _scheduler.ResumeAll();
        }

        public void ResumeJobGroup(string jobGroup)
        {
            _scheduler.ResumeJobs(GroupMatcher<JobKey>.GroupEquals(jobGroup));
        }

        public void ResumeJob(string jobName, string jobGroup)
        {
            _scheduler.ResumeJob(new JobKey(jobName, jobGroup));
        }

        public void ResumeTrigger(string triggerName, string triggerGroup)
        {
            _scheduler.ResumeTrigger(new TriggerKey(triggerName, triggerGroup));
        }

        public void StandbyScheduler()
        {
            _scheduler.Standby();
        }

        public void StartScheduler()
        {
            _scheduler.Start();
        }

        public void StopScheduler()
        {
            _scheduler.Shutdown(false);
        }

        /// <summary>
        /// 应用触发器数据
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="trigger"></param>
        /// <param name="triggerBuilder"></param>
        /// <returns></returns>
        private static TriggerBuilder ApplyTriggerData(string triggerName, TriggerType trigger, TriggerBuilder triggerBuilder)
        {
            if (!string.IsNullOrEmpty(triggerName))
            {
                triggerBuilder = triggerBuilder.WithIdentity(triggerName);
            }

            if (trigger is SimpleTriggerType simpleTrigger)
            {
                triggerBuilder = triggerBuilder.WithSimpleSchedule(x =>
                {
                    if (simpleTrigger.RepeatCount == -1)
                    {
                        x.RepeatForever();
                    }
                    else
                    {
                        x.WithRepeatCount(simpleTrigger.RepeatCount);
                    }

                    x.WithInterval(TimeSpan.FromMilliseconds(simpleTrigger.RepeatInterval));
                });
            }
            else if (trigger is CronTriggerType cronTriggerType)
            {
                triggerBuilder = triggerBuilder.WithCronSchedule(cronTriggerType.CronExpression);
            }
            else
            {
                throw new Exception("不支持的 trigger type: " + trigger.Code);
            }

            return triggerBuilder;
        }
    }
}