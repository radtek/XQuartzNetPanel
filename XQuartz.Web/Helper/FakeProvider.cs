namespace XQuartz.Web
{
    using System.Collections.Specialized;

    using CrystalQuartz.Core.Contracts;
    using CrystalQuartz.Core.SchedulerProviders;

    using Quartz;
    using Quartz.Impl;

    public class FakeProvider : ISchedulerProvider
    {
        /// <summary>
        /// 创建调度器
        /// </summary>
        /// <param name="engine">engine</param>
        /// <returns>myScheduler</returns>
        public object CreateScheduler(ISchedulerEngine engine)
        {
            NameValueCollection properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "RemoteClient";

            // 设置线程池
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            // 设置远程连接
            properties["quartz.scheduler.proxy"] = "true";
            properties["quartz.scheduler.proxy.address"] = "tcp://127.0.0.1:11111/QuartzScheduler";
            IScheduler myScheduler = new StdSchedulerFactory(properties).GetScheduler();

            return myScheduler;
        }
    }
}