using CrystalQuartz.Application.Comands;
using CrystalQuartz.Application.Comands.Serialization;
using CrystalQuartz.Core;
using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.SchedulerProviders;
using CrystalQuartz.WebFramework.Config;
using CrystalQuartz.WebFramework.Request;
using System;
using System.Reflection;

namespace CrystalQuartz.Application
{
    /// <summary>
    /// 水晶石英 面板应用
    /// </summary>
    public class CrystalQuartzPanelApplication : WebFramework.Application
    {
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly Options _options;

        /// <summary>
        /// 水晶石英 面板应用
        /// </summary>
        /// <param name="schedulerProvider"></param>
        /// <param name="options"></param>
        public CrystalQuartzPanelApplication(
            ISchedulerProvider schedulerProvider,
            Options options) :
            base(Assembly.GetAssembly(typeof(CrystalQuartzPanelApplication)),
                "CrystalQuartz.Application.Content.") //指定程序集 与 资源前缀
        {
            _schedulerProvider = schedulerProvider;
            _options = options;
        }

        public override IHandlerConfig Config
        {
            get
            {
                var initializer = new ShedulerHostInitializer(_schedulerProvider, _options);

                Func<SchedulerHost> hostProvider = () => initializer.SchedulerHost;

                if (!_options.LazyInit)
                {
                    //强迫的主机
                    var forcedHost = hostProvider.Invoke();
                }

                //调度器 数据输出序列化 对象
                var schedulerDataSerializer = new SchedulerDataOutputSerializer();

                return this

                    .WithHandler(new FileRequestHandler(Assembly.GetExecutingAssembly(), Context.DefautResourcePrefix))

                    /*
                     * trigger 命令
                     */
                    .WhenCommand("pause_trigger").Do(new PauseTriggerCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("resume_trigger").Do(new ResumeTriggerCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("delete_trigger").Do(new DeleteTriggerCommand(hostProvider), schedulerDataSerializer)

                    .WhenCommand("add_trigger").Do(new AddTriggerCommand(hostProvider, _options.JobDataMapInputTypes), new AddTriggerOutputSerializer())

                    /*
                     * Group 命令
                     */
                    .WhenCommand("pause_group").Do(new PauseGroupCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("resume_group").Do(new ResumeGroupCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("delete_group").Do(new DeleteGroupCommand(hostProvider), schedulerDataSerializer)

                    /*
                     * Job 命令
                     */
                    .WhenCommand("pause_job").Do(new PauseJobCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("resume_job").Do(new ResumeJobCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("delete_job").Do(new DeleteJobCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("execute_job").Do(new ExecuteNowCommand(hostProvider), schedulerDataSerializer)

                    /*
                     * Scheduler 命令
                     */
                    .WhenCommand("start_scheduler").Do(new StartSchedulerCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("stop_scheduler").Do(new StopSchedulerCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("get_scheduler_details").Do(new GetSchedulerDetailsCommand(hostProvider), new SchedulerDetailsOutputSerializer())
                    .WhenCommand("pause_scheduler").Do(new PauseAllCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("resume_scheduler").Do(new ResumeAllCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("standby_scheduler").Do(new StandbySchedulerCommand(hostProvider), schedulerDataSerializer)

                    /*
                     * Misc 命令
                     */
                    .WhenCommand("get_data").Do(new GetDataCommand(hostProvider), schedulerDataSerializer)
                    .WhenCommand("get_env").Do(new GetEnvironmentDataCommand(hostProvider, _options.CustomCssUrl, _options.TimelineSpan, _options.FrameworkVersion), new EnvironmentDataOutputSerializer())
                    .WhenCommand("get_job_details").Do(new GetJobDetailsCommand(hostProvider, _options.JobDataMapTraversingOptions), new JobDetailsOutputSerializer())
                    .WhenCommand("get_trigger_details").Do(new GetTriggerDetailsCommand(hostProvider, _options.JobDataMapTraversingOptions), new TriggerDetailsOutputSerializer())
                    .WhenCommand("get_input_types").Do(new GetInputTypesCommand(_options.JobDataMapInputTypes), new InputTypeOptionsSerializer())
                    .WhenCommand("get_input_type_variants").Do(new GetInputTypeVariantsCommand(_options.JobDataMapInputTypes), new InputTypeVariantOutputSerializer())
                    .WhenCommand("get_job_types").Do(new GetAllowedJobTypesCommand(hostProvider), new JobTypesOutputSerializer())

                    .Else()
                    .MapTo("index.html");
            }
        }
    }
}