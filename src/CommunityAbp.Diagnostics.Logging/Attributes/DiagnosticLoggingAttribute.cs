using Microsoft.Extensions.Logging;

namespace CommunityAbp.Diagnostics.Logging.Attributes
{
    /// <summary>
    ///     Attribute to mark methods or classes for diagnostic logging
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class DiagnosticLoggingAttribute : Attribute
    {
        /// <summary>
        ///     Whether to enable diagnostic logging for this method/class
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Whether to log stack trace for this method
        /// </summary>
        public bool LogStackTrace { get; set; } = false;

        /// <summary>
        ///     Whether to log Unit of Work information
        /// </summary>
        public bool LogUnitOfWork { get; set; } = true;

        /// <summary>
        ///     Whether to log method arguments
        /// </summary>
        public bool LogArguments { get; set; } = true;

        /// <summary>
        ///     Custom log level for this method (if different from global setting)
        /// </summary>
        public LogLevel? LogLevel { get; set; }

        /// <summary>
        ///     Custom description or tag for this diagnostic entry
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Depth of stack trace to log (if LogStackTrace is true)
        /// </summary>
        public int StackTraceDepth { get; set; } = 10;
    }
}