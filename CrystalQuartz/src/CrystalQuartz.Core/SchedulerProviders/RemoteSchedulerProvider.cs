using CrystalQuartz.Core.Contracts;

namespace CrystalQuartz.Core.SchedulerProviders
{
    /// <summary>
    /// 远程 调度器 提供者
    /// </summary>
    public class RemoteSchedulerProvider : ISchedulerProvider
    {
        public string SchedulerHost { get; set; }

        public object CreateScheduler(ISchedulerEngine engine)
        {
            return engine.CreateStandardRemoteScheduler(SchedulerHost);
        }
    }
}