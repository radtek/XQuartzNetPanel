namespace CrystalQuartz.Application
{
    using System;
    using System.Globalization;
    using CrystalQuartz.Core.Domain.ObjectInput;
    using CrystalQuartz.Core.Domain.ObjectTraversing;

    /// <summary>
    /// Public Crystal Quartz Panel options.
    /// </summary>
    public class CrystalQuartzOptions
    {
        public static RegisteredInputType[] CreateDefaultJobDataMapInputTypes()
        {
            var cultureInfo = CultureInfo.InvariantCulture;

            return new[]
            {
                new RegisteredInputType(
                    new InputType("String"), null),
                new RegisteredInputType(
                    new InputType("Int"), new StandardInputTypeConverter(cultureInfo, TypeCode.Int32)),
                new RegisteredInputType(
                    new InputType("Long"), new StandardInputTypeConverter(cultureInfo, TypeCode.Int64)),
                new RegisteredInputType(
                    new InputType("Float"), new StandardInputTypeConverter(cultureInfo, TypeCode.Single)),
                new RegisteredInputType(
                    new InputType("Double"), new StandardInputTypeConverter(cultureInfo, TypeCode.Double)),
                new RegisteredInputType(
                    new InputType("Boolean"),
                    new StandardInputTypeConverter(cultureInfo, TypeCode.Boolean),
                    new FixedInputVariantsProvider(
                        new InputVariant(true.ToString(cultureInfo), "True"),
                        new InputVariant(false.ToString(cultureInfo), "False")))
            };
        }

        /// <summary>
        /// 获取或设置面板URL路径。默认为“/quartz”
        /// </summary>
        public string Path { get; set; }

        public string CustomCssUrl { get; set; }

        /// <summary>
        ///获取或设置一个标志，该标志指示CrystalQuartz面板应在应用程序启动后立即初始化（<code>false<code>）还是在首次调用面板服务后立即初始化（<code>true<code>）。
        /// </summary>
        public bool LazyInit { get; set; } = false;

        public TimeSpan TimelineSpan { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// 获取或设置控制作业详细信息对象图形显示的选项。
        /// </summary>
        public JobDataMapDisplayOptions JobDataMapDisplayOptions { get; set; }

        public RegisteredInputType[] JobDataMapInputTypes { get; set; } = CreateDefaultJobDataMapInputTypes();

        /// <summary>
        /// 获取或设置job失败检测的选项。
        /// </summary>
        public ErrorDetectionOptions ErrorDetectionOptions { get; set; }

        /// <summary>
        /// 获取或设置用于job结果分析的选项。
        /// </summary>
        public JobResultAnalyserOptions JobResultAnalyserOptions { get; set; }

        /// <summary>
        ///允许用户计划任意作业是不安全的。此列表定义允许计划的作业类型。代码中计划的任何作业都假定是允许的，并且不需要出现在列表中。
        /// </summary>
        public Type[] AllowedJobTypes { get; set; }
    }

    public class ConfigurableTraversingOptions
    {
        public int MaxGraphDepth { get; set; } = 3;

        public int MaxPropertiesCount { get; set; } = 10;

        public int MaxEnumerableLength { get; set; } = 10;

        internal TraversingOptions ToTraversingOptions()
        {
            return new TraversingOptions(MaxGraphDepth, MaxPropertiesCount, MaxEnumerableLength);
        }
    }

    public class JobDataMapDisplayOptions : ConfigurableTraversingOptions { }

    public class ErrorDetectionOptions
    {
        /// <summary>
        /// Gets or sets a sources of error detection.
        /// </summary>
        public ErrorExtractionSource Source { get; set; } = ErrorExtractionSource.UnhandledExceptions | ErrorExtractionSource.JobResult;

        /// <summary>
        /// Gets or sets verbocity level for exception messages.
        /// </summary>
        public ErrorVerbosityLevel VerbosityLevel { get; set; } = ErrorVerbosityLevel.Minimal;

        /// <summary>
        /// Maximum length of exception message to store.
        /// </summary>
        public int ExceptionMessageLengthLimit { get; set; } = 200;
    }

    public abstract class JobResultAnalyserOptions { }

    public class DictionaryJobResultAnalyzerOptions : JobResultAnalyserOptions
    {
        public string SuccessKey { get; set; } = "Success";

        public string FailedKey { get; set; } = "Failed";

        public string ExceptionKey { get; set; } = "Error";
    }

    [Flags]
    public enum ErrorExtractionSource : short
    {
        /// <summary>
        /// Do not extract and store exceptions.
        /// </summary>
        None = 0,

        /// <summary>
        /// Extract only unhandled jobs exceptions reported by scheduler.
        /// </summary>
        UnhandledExceptions = 1,

        /// <summary>
        /// Extract exceptions from job results.
        /// </summary>
        JobResult = 2
    }

    public enum ErrorVerbosityLevel
    {
        /// <summary>
        /// Only mark the fire as failed without any error details.
        /// </summary>
        None,

        /// <summary>
        /// Provide message of top-level exception, trancate it if too long.
        /// </summary>
        Minimal,

        /// <summary>
        /// Provide recursive messages for all the inner exceptions.
        /// </summary>
        Detailed
    }
}