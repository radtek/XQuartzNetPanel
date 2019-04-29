using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace Job3
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class Job : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                System.Console.WriteLine(DateTime.Now + "  Job3执行成功");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Job3 发生异常:" + e.Message);
            }
        }
    }
}
