using System.Diagnostics;
using System.Reflection;
using CommunityAbp.Diagnostics.Logging.Attributes;
using CommunityAbp.Diagnostics.Logging.Configuration;
using CommunityAbp.Diagnostics.Logging.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.Uow;

namespace CommunityAbp.Diagnostics.Logging.Interceptors
{
    /// <summary>
    ///     Enhanced interceptor for logging diagnostic information about application service method invocations.
    ///     Supports both attribute-based selective logging and global configuration.
    /// </summary>
    public class ApplicationServiceDiagnosticsInterceptor : IAbpInterceptor, ITransientDependency
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApplicationServiceDiagnosticsInterceptor> _logger;
        private readonly DiagnosticOptions _options;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationServiceDiagnosticsInterceptor" /> class.
        /// </summary>
        /// <param name="logger">
        ///     The logger used to log diagnostic information. This should be injected by the DI container.
        /// </param>
        /// <param name="httpContextAccessor">
        ///     The HTTP context accessor to retrieve the current HTTP context, if available.
        /// </param>
        /// <param name="unitOfWorkManager">
        ///     The unit of work manager to access the current unit of work context, if applicable.
        /// </param>
        /// <param name="options">
        ///     Optional diagnostic options to configure the interceptor's behavior. If not provided, defaults will be used.
        /// </param>
        public ApplicationServiceDiagnosticsInterceptor(
            ILogger<ApplicationServiceDiagnosticsInterceptor> logger,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWorkManager unitOfWorkManager,
            DiagnosticOptions? options = null)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWorkManager = unitOfWorkManager;
            _options = options ?? new DiagnosticOptions();
        }

        /// <summary>
        ///     Intercepts method invocations on application services to log diagnostic information.
        /// </summary>
        /// <param name="invocation">
        ///     The method invocation context containing details about the method being invoked,
        /// </param>
        public async Task InterceptAsync(IAbpMethodInvocation invocation)
        {
            var methodConfig = GetMethodDiagnosticConfig(invocation.Method);

            if (!methodConfig.Enabled)
            {
                await invocation.ProceedAsync();
                return;
            }

            var diagContext = CreateDiagnosticContext(invocation);
            LogAppServiceStart(diagContext, methodConfig);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await invocation.ProceedAsync();
                stopwatch.Stop();
                LogAppServiceEnd(diagContext, stopwatch.ElapsedMilliseconds, true, methodConfig);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogAppServiceEnd(diagContext, stopwatch.ElapsedMilliseconds, false, methodConfig, ex);
                throw;
            }
        }

        /// <summary>
        ///     Determines the diagnostic configuration for a specific method by checking for attributes
        ///     and merging with global options.
        /// </summary>
        private MethodDiagnosticConfig GetMethodDiagnosticConfig(MethodInfo method)
        {
            // If global options require all diagnostics to be disabled, return early
            if (!_options.EnableDiagnostics) return new MethodDiagnosticConfig { Enabled = false };

            // Check for method-level attribute first
            var methodAttribute = method.GetCustomAttribute<DiagnosticLoggingAttribute>();
            if (methodAttribute != null) return MethodDiagnosticConfig.Merge(_options, methodAttribute);

            // Check for class-level attribute
            var classAttribute = method.DeclaringType?.GetCustomAttribute<DiagnosticLoggingAttribute>();
            if (classAttribute != null) return MethodDiagnosticConfig.Merge(_options, classAttribute);

            // Check for interface-level attribute (useful for ABP application services)
            foreach (var interfaceType in method.DeclaringType?.GetInterfaces() ?? [])
            {
                var interfaceMethodAttribute = GetInterfaceMethodAttribute(interfaceType, method);
                if (interfaceMethodAttribute != null)
                    return MethodDiagnosticConfig.Merge(_options, interfaceMethodAttribute);

                var interfaceAttribute = interfaceType.GetCustomAttribute<DiagnosticLoggingAttribute>();
                if (interfaceAttribute != null) return MethodDiagnosticConfig.Merge(_options, interfaceAttribute);
            }

            // If RequireAttribute is true and no attribute found, disable diagnostics
            return _options.RequireAttribute
                ? new MethodDiagnosticConfig { Enabled = false }
                : MethodDiagnosticConfig.FromOptions(_options); // Default to global options
        }

        /// <summary>
        ///     Finds the corresponding method in an interface and checks for diagnostic attributes
        /// </summary>
        private static DiagnosticLoggingAttribute? GetInterfaceMethodAttribute(Type interfaceType,
            MethodInfo implementationMethod)
        {
            try
            {
                var interfaceMethod = interfaceType.GetMethod(
                    implementationMethod.Name,
                    implementationMethod.GetParameters().Select(p => p.ParameterType).ToArray()
                );

                return interfaceMethod?.GetCustomAttribute<DiagnosticLoggingAttribute>();
            }
            catch
            {
                // If we can't find the interface method, just return null
                return null;
            }
        }


        private DiagnosticContext CreateDiagnosticContext(IAbpMethodInvocation invocation)
        {
            var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
                                ?? "no-http-context";

            return new DiagnosticContext
            {
                CorrelationId = correlationId,
                LayerType = "APP_SERVICE",
                MethodName = invocation.Method.Name,
                TypeName = invocation.TargetObject.GetType().Name,
                CustomData = new Dictionary<string, object>
                {
                    ["IsProxy"] = invocation.TargetObject.GetType().Namespace?.Contains("Castle.Proxies") == true,
                    ["MethodHash"] = invocation.Method.GetHashCode(),
                    ["Arguments"] = GetMethodArguments(invocation)
                }
            };
        }

        private void LogAppServiceStart(DiagnosticContext context, MethodDiagnosticConfig config)
        {
            var layerType = !string.IsNullOrEmpty(config.Description)
                ? $"{context.LayerType} ({config.Description})"
                : context.LayerType;

            _logger.Log(config.LogLevel, "=== {LayerType} START ===", layerType);
            _logger.Log(config.LogLevel,
                "RequestId: {RequestId}, CorrelationId: {CorrelationId}, Time: {Time:HH:mm:ss.fff}",
                context.RequestId, context.CorrelationId, context.StartTime);
            _logger.Log(config.LogLevel,
                "Method: {TypeName}.{MethodName}, Hash: {Hash}",
                context.TypeName, context.MethodName, context.CustomData["MethodHash"]);
            _logger.Log(config.LogLevel,
                "Is Proxy: {IsProxy}",
                context.CustomData["IsProxy"]);

            if (config.LogUnitOfWork)
            {
                var currentUow = _unitOfWorkManager.Current;
                _logger.Log(config.LogLevel,
                    "UOW ID: {UowId}, UOW IsActive: {IsActive}",
                    currentUow?.Id, currentUow?.IsCompleted == false);
            }

            // Log method arguments
            if (config.LogArguments)
            {
                var args = (Dictionary<string, string>)context.CustomData["Arguments"];
                if (args.Count != 0)
                    _logger.Log(config.LogLevel, "Arguments: {Arguments}",
                        string.Join(", ", args.Select(kv => $"{kv.Key}={kv.Value}")));
            }

            if (config.LogStackTrace)
            {
                var stackTrace = new StackTrace(true);
                var frames = stackTrace.GetFrames()
                    .Take(config.StackTraceDepth)
                    .Select(f => $"{f.GetMethod()?.DeclaringType?.Name}.{f.GetMethod()?.Name}")
                    .ToList();

                _logger.Log(config.LogLevel, "Call Stack: {Stack}", string.Join(" -> ", frames));
            }
        }

        private void LogAppServiceEnd(DiagnosticContext context, long durationMs, bool success,
            MethodDiagnosticConfig config, Exception? exception = null)
        {
            var layerType = !string.IsNullOrEmpty(config.Description)
                ? $"{context.LayerType} ({config.Description})"
                : context.LayerType;

            _logger.Log(config.LogLevel, "=== {LayerType} END ===", layerType);
            _logger.Log(config.LogLevel,
                "RequestId: {RequestId}, Duration: {Duration}ms, Success: {Success}",
                context.RequestId, durationMs, success);

            if (exception != null)
                _logger.LogError(exception,
                    "RequestId: {RequestId} - Exception in Application Service {TypeName}.{MethodName}",
                    context.RequestId, context.TypeName, context.MethodName);
        }

        private static Dictionary<string, string> GetMethodArguments(IAbpMethodInvocation invocation)
        {
            var result = new Dictionary<string, string>();
            var parameters = invocation.Method.GetParameters();

            for (var i = 0; i < parameters.Length && i < invocation.Arguments.Length; i++)
            {
                var paramName = parameters[i].Name;
                var argValue = invocation.Arguments[i];

                if (paramName == null) continue; // Skip if parameter name is null
                result[paramName] = FormatArgumentValue(argValue) ?? string.Empty;
            }

            return result;
        }

        private static string? FormatArgumentValue(object? argValue)
        {
            if (argValue == null) return "null";

            var type = argValue.GetType();

            // Handle primitive types and strings
            if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(Guid))
                return argValue.ToString();

            // For complex objects, try to extract common properties like Skip, Max, etc.
            var skipCount = type.GetProperty("SkipCount")?.GetValue(argValue);
            var maxResult = type.GetProperty("MaxResultCount")?.GetValue(argValue);
            var sorting = type.GetProperty("Sorting")?.GetValue(argValue);

            if (skipCount != null || maxResult != null || sorting != null)
                return $"Skip={skipCount}, Max={maxResult}, Sort={sorting}";

            return $"[{type.Name}]";
        }
    }
}