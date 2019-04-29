namespace CrystalQuartz.Web
{
    using System.Web;
    using CrystalQuartz.Application;
    using CrystalQuartz.Application.Startup;
    using CrystalQuartz.Core.SchedulerProviders;
    using CrystalQuartz.WebFramework;
    using CrystalQuartz.WebFramework.SystemWeb;

    /// <summary>
    /// 页面  处理程序
    /// </summary>
    public class PagesHandler : IHttpHandler
    {
        private static readonly RunningApplication RunningApplication;

        /// <summary>
        /// 静态构造
        /// </summary>
        static PagesHandler()
        {
            var options = new CrystalQuartzOptions
            {
                //css地址
                CustomCssUrl = Configuration.ConfigUtils.CustomCssUrl
            };

            //调度器
            ISchedulerProvider schedulerProvider = Configuration.ConfigUtils.SchedulerProvider;

            //初始化 应用程序
            Application application = new CrystalQuartzPanelApplication(
                schedulerProvider,
                options.ToRuntimeOptions(SchedulerEngineProviders.SchedulerEngineResolvers, FrameworkVersion.Value));

            //运行
            RunningApplication = application.Run();
        }

        public void ProcessRequest(HttpContext context)
        {
            RunningApplication.Handle(
                new SystemWebRequest(context),
                new SystemWebResponseRenderer(context));
        }

        public virtual bool IsReusable
        {
            get { return false; }
        }
    }
}