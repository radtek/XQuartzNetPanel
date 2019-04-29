namespace CrystalQuartz.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Web.Configuration;
    using CrystalQuartz.Core.SchedulerProviders;

    public static class ConfigUtils
    {
        /// <summary>
        /// 获取css地址
        /// </summary>
        public static string CustomCssUrl
        {
            get
            {
                CrystalQuartzOptionsSection section = (CrystalQuartzOptionsSection)WebConfigurationManager.GetSection("crystalQuartz/options");
                if (section == null)
                {
                    return null;
                }

                return section.CustomCssUrl;
            }
        }

        /// <summary>
        /// 实例化 调度器类
        /// </summary>
        public static ISchedulerProvider SchedulerProvider
        {
            get
            {
                var section = (Hashtable) WebConfigurationManager.GetSection("crystalQuartz/provider");
                var type = Type.GetType(section["Type"].ToString());
                //通过反射实例化该类 (FakeProvider)
                var provider = Activator.CreateInstance(type);
                foreach (string property in section.Keys)
                {
                    if (property != "Type")
                    {
                        provider.GetType().GetProperty(property).SetValue(provider, section[property], new object[]{});
                    }
                }

                return (ISchedulerProvider) provider;
            }
        }
    }
}