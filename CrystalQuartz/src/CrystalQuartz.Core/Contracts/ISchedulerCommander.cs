using CrystalQuartz.Core.Domain.TriggerTypes;

namespace CrystalQuartz.Core.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface ISchedulerCommander
    {
        /// <summary>
        ///  新增一个触发器给 现有的job
        /// </summary>
        /// <param name="jobName">existing job name</param>
        /// <param name="jobGroup">existing job group</param>
        /// <param name="triggerName">new trigger name or <code>Null</code></param>
        /// <param name="trigger">trigger type data</param>
        /// <param name="jobData">trigger job data map (merges with original job's job data map)</param>
        void ScheduleJob(
            string jobName, 
            string jobGroup, 
            string triggerName,
            TriggerType trigger,
            IDictionary<string, object> jobData);

        /// <summary>
        /// 新增一个job 并添加一个 触发器
        /// </summary>
        /// <param name="jobName">new job name</param>
        /// <param name="jobGroup">new or existing job name</param>
        /// <param name="jobType">job type</param>
        /// <param name="triggerName">new trigger name or <code>Null</code></param>
        /// <param name="triggerType">trigger type data</param>
        /// <param name="jobData">job data map</param>
        void ScheduleJob(
            string jobName,
            string jobGroup,
            Type jobType,
            string triggerName,
            TriggerType triggerType,
            IDictionary<string, object> jobData);

        void DeleteJobGroup(string jobGroup);
        void DeleteJob(string jobName, string jobGroup);
        void DeleteTrigger(string triggerName, string triggerGroup);

        void ExecuteNow(string jobName, string jobGroup);

        void PauseAllJobs();
        void PauseJobGroup(string jobGroup);
        void PauseJob(string jobName, string jobGroup);
        void PauseTrigger(string triggerName, string triggerGroup);

        void ResumeAllJobs();
        void ResumeJobGroup(string jobGroup);
        void ResumeJob(string jobName, string jobGroup);
        void ResumeTrigger(string triggerName, string triggerGroup);

        void StandbyScheduler();
        void StartScheduler();
        void StopScheduler();
    }
}