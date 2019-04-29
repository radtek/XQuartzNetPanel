using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.Quartz2;
using CrystalQuartz.Core.Quartz3;
using System;
using System.Collections.Generic;

namespace CrystalQuartz.Web
{
    /// <summary>
    /// 调度器引擎提供者 (此类库版本为 4.5.2 所以也支持 3.x 的 Quartz)
    /// </summary>
    public static class SchedulerEngineProviders
    {
        /// <summary>
        /// 调度器引擎 解析器 (容器)
        /// </summary>
        public static readonly IDictionary<int, Func<ISchedulerEngine>> SchedulerEngineResolvers = new Dictionary<int, Func<ISchedulerEngine>>
        {
            { 2, () => new Quartz2SchedulerEngine() },
            { 3, () => new Quartz3SchedulerEngine() }
        };
    }
}