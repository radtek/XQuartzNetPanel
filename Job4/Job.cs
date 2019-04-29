using System;

using Quartz;

namespace Job4
{
  
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class Job : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //使用时请将该dll 手动复制到 QuartzService 与 WebPanel  俩个项目中 的根目录
                //才可以添加job
                var map = context.MergedJobDataMap;
                var name = string.Empty;
                if (map.ContainsKey("name"))
                {
                    name = map.GetString("name");
                }

                System.Console.WriteLine(DateTime.Now + "  Job4执行成功: name -> " + name);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Job4 发生异常:" + e.Message);
            }
        }
    }
}