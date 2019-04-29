using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.SchedulerProviders;
using Quartz.Impl;

namespace CrystalQuartz.Web.Demo
{
    using Quartz;
    using Quartz.Collection;
    using System.Collections.Specialized;

    public class FakeProvider : ISchedulerProvider
    {
        /// <summary>
        /// 创建调度器
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        public object CreateScheduler(ISchedulerEngine engine)
        {
            //配置
            NameValueCollection properties = new NameValueCollection();
            properties.Add("test1", "test1value");
            properties.Add("quartz.scheduler.instanceName", "xxxue 的quartz");
            properties.Add("quartz.scheduler.instanceId", "test|pipe");
          
            //实例化 调度器
            var scheduler = new StdSchedulerFactory(properties)?.GetScheduler();

            /*
             添加job与触发器的几种格式
             */

            #region job1

            // 构造 job
            var jobDetail = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob") //job 唯一名称
                .StoreDurably() //持久储存
                .Build();

            // 间隔1分钟执行
            var trigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger") //触发器 唯一名称
                .StartNow() //立即开始
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever()) //简单调度: 间隔一分钟 (RepeatForever 重复 永远 :指定触发器将无限期重复。)
                .Build(); //创建这个触发器

            scheduler.ScheduleJob(jobDetail, trigger);//放入 job与触发器

            #endregion job1

            #region job2

            // construct job info
            var jobDetail2 = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob2")
                .Build();

            // fire every 3 minutes
            var trigger2 = TriggerBuilder.Create()
                .WithIdentity("myTrigger2")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(3))
                .Build();

            scheduler.ScheduleJob(jobDetail2, trigger2);

            var trigger3 = TriggerBuilder.Create()
               .WithIdentity("myTrigger3")
               .ForJob(jobDetail2) //指定 对应的 job
               .StartNow()
               .WithSimpleSchedule(x => x.WithIntervalInSeconds(40).RepeatForever())
               //.WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever())
               .Build();

            scheduler.ScheduleJob(trigger3);

            #endregion job2

            #region job4

            // construct job info
            var jobDetail4 = JobBuilder.Create<HelloJob>()
                .WithIdentity("myJob4", "MyOwnGroup")
                .Build();

            jobDetail4.JobDataMap.Add("key1", "value1");
            jobDetail4.JobDataMap.Add("key2", "value2");
            jobDetail4.JobDataMap.Add("key3", 1L);
            jobDetail4.JobDataMap.Add("key4", 1d);
            jobDetail4.JobDataMap.Add("key5", new { FisrtName = "John", LastName = "Smith" });
            jobDetail4.JobDataMap.Add("key6", new[]
            {
                new { Name = "test1" },
                new { Name = "test2" }
            });

            // fire every hour
            ITrigger trigger4 = TriggerBuilder.Create()
                .WithIdentity("myTrigger4", jobDetail4.Key.Group)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1))
                .Build();

            ITrigger trigger5 = TriggerBuilder.Create()
                .WithIdentity("myTrigger5", jobDetail4.Key.Group)
                .StartNow()
                .WithCronSchedule("0 0/5 * * * ?") //Cron 调度
                .Build();

            // job , 触发器, 存在相同的是否 替换 如果已存在  并且 Replact = false  则 抛出异常
            scheduler.ScheduleJob(jobDetail4, new HashSet<ITrigger>(new[] { trigger4, trigger5 }), false);
            //scheduler.ScheduleJob(jobDetail4, trigger5);

            #endregion job4

            //暂停 job
            scheduler.PauseJob(new JobKey("myJob4", "MyOwnGroup"));
            //暂停 触发器
            scheduler.PauseTrigger(new TriggerKey("myTrigger3", "DEFAULT"));

            //调度器开始
            scheduler.Start();

            return scheduler;
        }
    }
}