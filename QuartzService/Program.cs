using Quartz;
using Quartz.Impl;

namespace QuartzService
{
    internal class Program
    {
        private static IScheduler _scheduler = new StdSchedulerFactory().GetScheduler();

        private static void Main(string[] args)
        {          
            _scheduler.Start();
        }
    }
}