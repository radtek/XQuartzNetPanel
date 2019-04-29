using CrystalQuartz.Core;
using CrystalQuartz.Core.Contracts;
using CrystalQuartz.Core.SchedulerProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CrystalQuartz.Application
{
    using System.CodeDom;
    using CrystalQuartz.Core.Services;

    /// <summary>
    /// 调度器 主机 初始化
    /// </summary>
    public class ShedulerHostInitializer
    {
        private readonly object _lock = new object();

        /// <summary>
        /// 调度器 提供者
        /// </summary>
        private readonly ISchedulerProvider _schedulerProvider;

        private readonly Options _options;

        /// <summary>
        /// 调度器 引擎
        /// </summary>
        private ISchedulerEngine _schedulerEngine;

        /// <summary>
        /// 是否创建过
        /// </summary>
        private bool _valueCreated;

        /// <summary>
        /// 调度器
        /// </summary>
        private object _scheduler;

        private SchedulerHost _schedulerHost;

        /// <summary>
        /// 调度器 主机
        /// </summary>
        public SchedulerHost SchedulerHost
        {
            get
            {
                if (!_valueCreated)
                {
                    lock (_lock)
                    {
                        if (!_valueCreated)
                        {
                            Assembly quartzAssembly = FindQuartzAssembly();

                            if (quartzAssembly == null)
                            {
                                return AssignErrorHost("无法确定Quartz.net版本。请确保宿主项目引用了Quartz程序集。");
                            }

                            //Quartz版本
                            Version quartzVersion = quartzAssembly.GetName().Version;

                            //创建调度器引擎
                            if (_schedulerEngine == null)
                            {
                                try
                                {
                                    _schedulerEngine = CreateSchedulerEngineBy(quartzVersion);
                                    if (_schedulerEngine == null)
                                    {
                                        return AssignErrorHost("无法创建 Quartz.NET 引擎  版本:" + quartzVersion, quartzVersion);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return AssignErrorHost("无法为提供的 Quartz.NET 创建 引擎 版本:" + quartzVersion, quartzVersion, ex);
                                }
                            }

                            //创建调度器
                            if (_scheduler == null)
                            {
                                try
                                {
                                    //创建调度器
                                    _scheduler = _schedulerProvider.CreateScheduler(_schedulerEngine);
                                }
                                catch (FileLoadException ex)
                                {
                                    return AssignErrorHost(GetFileLoadingErrorMessage(ex, quartzVersion, quartzAssembly), quartzVersion);
                                }
                                catch (Exception ex)
                                {
                                    return AssignErrorHost("实例化 调度器 时出错。请检查 调度器 初始化代码。", quartzVersion, ex);
                                }
                            }

                            //调度器服务
                            SchedulerServices services;
                            //事件处理者
                            EventsTransformer eventsTransformer;

                            try
                            {
                                //创建调度器服务
                                services = _schedulerEngine.CreateServices(_scheduler, _options);
                                eventsTransformer = new EventsTransformer(_options.ExceptionTransformer, _options.JobResultAnalyser);
                            }
                            catch (FileLoadException ex)
                            {
                                return AssignErrorHost(GetFileLoadingErrorMessage(ex, quartzVersion, quartzAssembly), quartzVersion);
                            }
                            catch (Exception ex)
                            {
                                return AssignErrorHost("初始化 scheduler services时出错", quartzVersion, ex);
                            }

                            //事件中心
                            var eventHub = new SchedulerEventHub(1000, _options.TimelineSpan, eventsTransformer);
                            if (services.EventSource != null)
                            {
                                services.EventSource.EventEmitted += (sender, args) =>
                                {
                                    eventHub.Push(args.Payload);
                                };
                            }

                            //调度器主机
                            _schedulerHost = new SchedulerHost(
                                services.Clerk,
                                services.Commander,
                                quartzVersion,
                                eventHub,
                                eventHub,
                                new AllowedJobTypesRegistry(_options.AllowedJobTypes, services.Clerk));

                            _valueCreated = true;
                        }
                    }
                }

                return _schedulerHost;
            }
        }

        /// <summary>
        /// 调度器 主机 初始化 构造函数
        /// </summary>
        /// <param name="schedulerProvider"></param>
        /// <param name="options"></param>
        public ShedulerHostInitializer(ISchedulerProvider schedulerProvider, Options options)
        {
            //调度器提供者
            _schedulerProvider = schedulerProvider;
            _options = options;
        }

        /// <summary>
        /// 获取文件加载错误消息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="quartzVersion"></param>
        /// <param name="quartzAssembly"></param>
        /// <returns></returns>
        private string GetFileLoadingErrorMessage(FileLoadException exception, Version quartzVersion, Assembly quartzAssembly)
        {
            if (exception.FileName.StartsWith("Quartz,"))
            {
                string[] fileNameParts = exception.FileName.Split(',', '=');
                if (fileNameParts.Length > 2 && fileNameParts[1].Trim().Equals("Version", StringComparison.InvariantCultureIgnoreCase))
                {
                    string expectedVersion = fileNameParts[2];

                    return string.Format(
@"Quartz.NET version mismatch detected. CrystalQuartz expects v{0} but {1} was found in the host application. Consider adding the following bindings to the .config file:

<dependentAssembly>
    <assemblyIdentity name=""Quartz"" publicKeyToken=""{2}"" culture=""neutral""/>
    <bindingRedirect oldVersion=""0.0.0.0-{3}.9.9.9"" newVersion=""{1}""/>
</dependentAssembly>", expectedVersion, quartzVersion, GetPublicKeyTokenFromAssembly(quartzAssembly), quartzVersion.Major);
                }
            }

            return exception.Message;
        }

        /// <summary>
        /// 分配错误主机
        /// </summary>
        /// <param name="primaryError"></param>
        /// <param name="version"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private SchedulerHost AssignErrorHost(string primaryError, Version version = null, Exception exception = null)
        {
            _schedulerHost = new SchedulerHost(version, new[] { primaryError }.Concat(GetExceptionMessages(exception)).ToArray());

            return _schedulerHost;
        }

        /// <summary>
        /// 获取异常消息
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private IEnumerable<string> GetExceptionMessages(Exception exception)
        {
            if (exception == null)
            {
                yield break;
            }
            
            yield return exception.Message;

            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                foreach (var innerMessage in aggregateException.InnerExceptions.SelectMany(GetExceptionMessages))
                {
                    yield return innerMessage;
                }
            }
            else if (exception.InnerException != null)
            {
                foreach (var exceptionMessage in GetExceptionMessages(exception))
                {
                    yield return exceptionMessage;
                }
            }
        }

        /// <summary>
        /// 创建调度器引擎
        /// </summary>
        /// <param name="quartzVersion"></param>
        /// <returns></returns>
        private ISchedulerEngine CreateSchedulerEngineBy(Version quartzVersion)
        {
            if (!_options.SchedulerEngineResolvers.ContainsKey(quartzVersion.Major))
            {
                return null;
            }
            //从容器 取出,并 调用 委托
            return _options.SchedulerEngineResolvers[quartzVersion.Major].Invoke(); // todo
        }

        /// <summary>
        /// 获取quartz.dll 中的Quartz.IScheduler 程序集
        /// </summary>
        /// <returns></returns>
        private Assembly FindQuartzAssembly()
        {
            Type quartzSchedulerType = Type.GetType("Quartz.IScheduler, Quartz");
            if (quartzSchedulerType == null)
            {
                return null;
            }
            //返回程序集
            return quartzSchedulerType.Assembly;
        }

        /// <summary>
        /// 从程序集获取公钥标记
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static string GetPublicKeyTokenFromAssembly(Assembly assembly)
        {

            byte[] bytes = assembly.GetName().GetPublicKeyToken();
            if (bytes == null || bytes.Length == 0)
            {
                return "None";
            }

            var publicKeyToken = string.Empty;
            for (int i = 0; i < bytes.GetLength(0); i++)
            {
                //拼接
                publicKeyToken += string.Format("{0:x2}", bytes[i]);
            }

            return publicKeyToken;
        }
    }
}