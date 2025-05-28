using CommunityAbp.Diagnostics.Logging.Attributes;
using Microsoft.Extensions.Logging;

namespace CommunityAbp.Diagnostics.Logging.Configuration
{
    /// <summary>
    ///     Method-specific diagnostic configuration combining global options with attribute overrides
    /// </summary>
    public class MethodDiagnosticConfig
    {
        /// <summary>
        ///     Whether to enable diagnostic logging for this method or class.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Whether to log stack trace for this method or class.
        /// </summary>
        public bool LogStackTrace { get; set; }

        /// <summary>
        ///     Whether to log Unit of Work information for this method or class.
        /// </summary>
        public bool LogUnitOfWork { get; set; }

        /// <summary>
        ///     Whether to log method arguments for this method or class.
        /// </summary>
        public bool LogArguments { get; set; }

        /// <summary>
        ///     The minimum log level for diagnostic messages for this method or class.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        ///     Custom description or tag for this diagnostic entry.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        ///     Depth of stack trace to log for this method or class, if LogStackTrace is true.
        /// </summary>
        public int StackTraceDepth { get; set; }

        /// <summary>
        ///     Creates a MethodDiagnosticConfig instance from global diagnostic options
        /// </summary>
        /// <param name="options">
        ///     Global diagnostic options to use as a base configuration.
        /// </param>
        /// <returns>
        ///     A MethodDiagnosticConfig instance initialized with global options.
        /// </returns>
        public static MethodDiagnosticConfig FromOptions(DiagnosticOptions options)
        {
            return new MethodDiagnosticConfig
            {
                Enabled = options.EnableDiagnostics,
                LogStackTrace = options.LogStackTrace,
                LogUnitOfWork = options.LogUnitOfWork,
                LogArguments = options.LogArguments,
                LogLevel = options.LogLevel,
                StackTraceDepth = options.StackTraceDepth
            };
        }

        /// <summary>
        ///     Merges global diagnostic options with method-specific attribute settings
        /// </summary>
        /// <param name="options">
        ///     Global diagnostic options to use as a base configuration.
        /// </param>
        /// <param name="attribute">
        ///     Method or class attribute that may override global settings.
        /// </param>
        /// <returns>
        ///     A MethodDiagnosticConfig instance that combines global options with attribute overrides.
        /// </returns>
        public static MethodDiagnosticConfig Merge(DiagnosticOptions options, DiagnosticLoggingAttribute attribute)
        {
            var config = FromOptions(options);

            // Override with attribute values where specified
            config.Enabled = attribute.Enabled;
            config.LogStackTrace = attribute.LogStackTrace;
            config.LogUnitOfWork = attribute.LogUnitOfWork;
            config.LogArguments = attribute.LogArguments;
            config.Description = attribute.Description ?? "";
            config.StackTraceDepth =
                attribute.StackTraceDepth > 0 ? attribute.StackTraceDepth : options.StackTraceDepth;

            if (attribute.LogLevel.HasValue) config.LogLevel = attribute.LogLevel.Value;

            return config;
        }
    }
}