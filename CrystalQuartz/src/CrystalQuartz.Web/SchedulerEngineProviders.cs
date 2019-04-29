using System;
using System.Collections.Generic;
using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.Quartz2;

namespace CrystalQuartz.Web
{
    /// <summary>
    /// 调度器引擎提供者 (3.x版本以上需要 类库版本 4.5.2)
    /// </summary>
    public static class SchedulerEngineProviders
    {
        public static readonly IDictionary<int, Func<ISchedulerEngine>> SchedulerEngineResolvers = new Dictionary<int, Func<ISchedulerEngine>>
        {
            { 2, () => new Quartz2SchedulerEngine() }
        };
    }
}