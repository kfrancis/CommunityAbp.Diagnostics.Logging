using Microsoft.Extensions.Logging;

namespace CommunityAbp.Diagnostics.Logging.Configuration
{
    /// <summary>
    ///     Configuration options for diagnostic logging.
    /// </summary>
    public class DiagnosticOptions
    {
        /// <summary>
        ///     Enables or disables diagnostic logging. If set to false, no diagnostics will be logged.
        /// </summary>
        public bool EnableDiagnostics { get; set; } = true;

        /// <summary>
        ///     Specifies whether to log the stack trace for exceptions. If true, the stack trace will be included in the logs.
        /// </summary>
        public bool LogStackTrace { get; set; } = false;

        /// <summary>
        ///     Specifies whether to log the headers of HTTP requests and responses. If true, headers will be included in the logs.
        /// </summary>
        public bool LogHeaders { get; set; } = true;

        /// <summary>
        ///     Specifies whether to log the details of unit of work operations. If true, unit of work details will be included in
        ///     the logs.
        /// </summary>
        public bool LogUnitOfWork { get; set; } = true;

        /// <summary>
        ///     Specifies the maximum depth of the stack trace to log. This is useful for limiting the amount of detail in the logs
        ///     while still capturing relevant information.
        /// </summary>
        public int StackTraceDepth { get; set; } = 10;

        /// <summary>
        ///     Specifies whether to log method arguments. If true, the arguments passed to methods will be included in the logs.
        /// </summary>
        public bool LogArguments { get; set; } = true;

        /// <summary>
        ///     Specifies the minimum log level for diagnostic messages. Only messages at this level or higher will be logged.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        ///     When true, diagnostic logging is applied to ALL methods regardless of attributes.
        ///     When false, only methods/classes with [DiagnosticLogging] attribute are logged.
        /// </summary>
        public bool RequireAttribute { get; set; } = false;
    }
}