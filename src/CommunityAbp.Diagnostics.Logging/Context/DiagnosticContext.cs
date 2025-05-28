namespace CommunityAbp.Diagnostics.Logging.Context
{
    /// <summary>
    ///     Represents the context for diagnostic logging, containing metadata about the request and operation.
    /// </summary>
    public class DiagnosticContext
    {
        /// <summary>
        ///     Unique identifier for the request, generated as a GUID. This helps in tracing the request across different layers
        ///     and services.
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        ///     Correlation ID for tracking related requests or operations across distributed systems. This is useful for
        ///     correlating logs from different services that are part of the same operation.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        ///     Timestamp indicating when the operation started. This is useful for measuring the duration of operations and for
        ///     logging purposes.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        ///     Layer type where the operation is being executed, such as "Application", "Domain", or "Infrastructure". This helps
        ///     in categorizing logs based on the layer of the application.
        /// </summary>
        public string LayerType { get; set; } = string.Empty;

        /// <summary>
        ///     Name of the method being executed, which is useful for identifying the specific operation in the logs.
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        ///     Type name of the class or service where the operation is being executed. This helps in identifying the source of
        ///     the log entry.
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        ///     Custom data dictionary for storing additional metadata related to the operation. This can include any key-value
        ///     pairs that are relevant to the diagnostic context, such as user IDs, session IDs, or other contextual information.
        /// </summary>
        public Dictionary<string, object> CustomData { get; set; } = new();
    }
}