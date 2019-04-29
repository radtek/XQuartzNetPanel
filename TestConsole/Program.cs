using System;
using System.Collections.Specialized;
using System.Threading;

using Quartz;
using Quartz.Impl;

namespace TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            RemoteClientExample.Run();
            Thread.Sleep(999999);
            Console.WriteLine("over");
            Console.ReadLine();
        }
    }

    public class RemoteClientExample
    {
        public static void Run()
        {
            NameValueCollection properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "RemoteClient";

            //设置线程池
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            //设置远程连接
            properties["quartz.scheduler.proxy"] = "true";
            properties["quartz.scheduler.proxy.address"] = "tcp://127.0.0.1:11111/QuartzScheduler";

            ISchedulerFactory sf = new StdSchedulerFactory(properties);
            IScheduler sched = sf.GetScheduler();

            IJobDetail job = JobBuilder.Create<SimpleJob>()
                .WithIdentity("remotelyAddedJob2", "default")
                .Build();

            JobDataMap map = job.JobDataMap;
            map.Put("msg", "信息");

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("remotelyAddedTrigger2", "default")
                .ForJob(job.Key)
                .WithCronSchedule("/5 * * ? * *")
                .Build();
            sched.ScheduleJob(job, trigger);

            Console.WriteLine("向服务器添加计划任务");
        }
    }

    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class SimpleJob : IJob
    {
        public const string Message = "msg";

        public virtual void Execute(IJobExecutionContext context)
        {
            try
            {
                System.Console.WriteLine(DateTime.Now + "  SimpleJob执行成功");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Job1 发生异常:" + e.Message);
            }

            //JobKey jobKey = context.JobDetail.Key;
            //string message = context.JobDetail.JobDataMap.GetString(Message);
            //Console.WriteLine("{0} 执行时间 {1}", jobKey, DateTime.Now.ToString());
            //Console.WriteLine("msg: {0}", message);
        }
    }
}