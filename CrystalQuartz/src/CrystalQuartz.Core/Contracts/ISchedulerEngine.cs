namespace CrystalQuartz.Core.Contracts
{
    public interface ISchedulerEngine
    {
        /// <summary>
        /// 创建标准远程 调度器
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        object CreateStandardRemoteScheduler(string url);

        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        SchedulerServices CreateServices(object scheduler, Options options);
    }
}