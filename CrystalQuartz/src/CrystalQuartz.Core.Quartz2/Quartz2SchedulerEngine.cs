[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CrystalQuartz.Core.Quartz2.Tests")]

namespace CrystalQuartz.Core.Quartz2
{
    using System;
    using System.Collections.Specialized;
    using CrystalQuartz.Core.Contracts;
    using Quartz;
    using Quartz.Impl;

    /// <summary>
    /// Quartz 2 调度器 引擎
    /// </summary>
    public class Quartz2SchedulerEngine : ISchedulerEngine
    {
        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="schedulerInstance"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public SchedulerServices CreateServices(object schedulerInstance, Options options)
        {
            IScheduler scheduler = schedulerInstance as IScheduler;

            if (scheduler == null)
            {
                throw new Exception("需要 Quartz 2 Scheduler 的实例");
            }

            return new SchedulerServices(
                new Quartz2SchedulerClerk(scheduler),
                new Quartz2SchedulerCommander(scheduler),
                CreateEventSource(scheduler, options));
        }

        /// <summary>
        /// 创建事件源
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private ISchedulerEventSource CreateEventSource(IScheduler scheduler, Options options)
        {
            if (!scheduler.GetMetaData().SchedulerRemote)
            {
                var result = new Quartz2SchedulerEventSource(options.ExtractErrorsFromUnhandledExceptions);
                scheduler.ListenerManager.AddTriggerListener(result);

                if (options.ExtractErrorsFromUnhandledExceptions)
                {
                    scheduler.ListenerManager.AddJobListener(result);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// 创建标准的远程 调度器
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public object CreateStandardRemoteScheduler(string url)
        {
            var properties = new NameValueCollection();
            //启动 调度器代理 
            properties["quartz.scheduler.proxy"] = "true";
            properties["quartz.scheduler.proxy.address"] = url;

            //返回 调度器
            return new StdSchedulerFactory(properties).GetScheduler();
        }
    }
}