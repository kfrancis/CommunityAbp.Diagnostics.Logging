using CommunityAbp.Diagnostics.Logging.Configuration;
using CommunityAbp.Diagnostics.Logging.Context;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommunityAbp.Diagnostics.Logging.Attributes
{
    /// <summary>
    ///     Attribute for logging diagnostic information about page handler executions in ASP.NET Core Razor Pages.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PageHandlerDiagnosticsAttribute : ActionFilterAttribute
    {
        private readonly DiagnosticOptions _options = new();

        /// <summary>
        ///     Executes before the action method is called, logging diagnostic information about the page handler execution.
        /// </summary>
        /// <param name="context">
        ///     The action executing context containing information about the current request and action being executed.
        /// </param>
        /// <param name="next">
        ///     The delegate to execute the next action in the pipeline after logging diagnostics.
        /// </param>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!_options.EnableDiagnostics)
            {
                await next();
                return;
            }

            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<PageHandlerDiagnosticsAttribute>>();
            var diagContext = CreateDiagnosticContext(context);

            // Store correlation ID for downstream services
            context.HttpContext.Items["DiagnosticContext"] = diagContext;
            context.HttpContext.Items["CorrelationId"] = diagContext.CorrelationId;

            LogPageHandlerStart(logger, diagContext, context);

            var executedContext = await next();

            LogPageHandlerEnd(logger, diagContext, executedContext);
        }

        /// <summary>
        ///     Creates a diagnostic context for the current page handler execution.
        /// </summary>
        /// <param name="context">
        ///     The action executing context containing information about the current request and action being executed.
        /// </param>
        /// <returns>
        ///     A <see cref="DiagnosticContext" /> instance populated with relevant information about the page handler execution.
        /// </returns>
        private static DiagnosticContext CreateDiagnosticContext(ActionExecutingContext context)
        {
            return new DiagnosticContext
            {
                CorrelationId = Guid.NewGuid().ToString("N")[..8],
                LayerType = "PAGE_HANDLER",
                MethodName = context.ActionDescriptor.DisplayName ?? "Unknown",
                TypeName = context.Controller.GetType().Name
            };
        }

        /// <summary>
        ///     Logs the start of the page handler execution, including correlation ID, time, handler, method, and any
        ///     DataSourceRequest details.
        /// </summary>
        /// <param name="logger">
        ///     The logger instance to log diagnostic information.
        /// </param>
        /// <param name="context">
        ///     The diagnostic context containing metadata about the request and operation.
        /// </param>
        /// <param name="actionContext">
        ///     The action executing context containing information about the current request and action being executed.
        /// </param>
        private void LogPageHandlerStart(ILogger logger, DiagnosticContext context,
            ActionExecutingContext actionContext)
        {
            logger.Log(_options.LogLevel, "=== {LayerType} START ===", context.LayerType);
            logger.Log(_options.LogLevel,
                "CorrelationId: {CorrelationId}, Time: {Time:HH:mm:ss.fff}",
                context.CorrelationId, context.StartTime);
            logger.Log(_options.LogLevel,
                "Handler: {Handler}, Method: {Method}",
                context.TypeName, context.MethodName);

            // Log any DataSourceRequest details if present
            var dataSourceRequest = actionContext.ActionArguments.Values
                .FirstOrDefault(arg => arg?.GetType().Name.Contains("DataSourceRequest") == true);

            if (dataSourceRequest == null)
            {
                return;
            }

            // Use reflection to get common properties
            var type = dataSourceRequest.GetType();
            var page = type.GetProperty("Page")?.GetValue(dataSourceRequest);
            var pageSize = type.GetProperty("PageSize")?.GetValue(dataSourceRequest);

            logger.Log(_options.LogLevel,
                "DataSource: Page={Page}, PageSize={PageSize}",
                page, pageSize);
        }

        /// <summary>
        ///     Logs the end of the page handler execution, including correlation ID and duration.
        /// </summary>
        /// <param name="logger">
        ///     The logger instance to log diagnostic information.
        /// </param>
        /// <param name="context">
        ///     The diagnostic context containing metadata about the request and operation.
        /// </param>
        /// <param name="executedContext">
        ///     The action executed context containing information about the completed action execution.
        /// </param>
        private void LogPageHandlerEnd(ILogger logger, DiagnosticContext context, ActionExecutedContext executedContext)
        {
            var duration = DateTime.Now - context.StartTime;

            logger.Log(_options.LogLevel, "=== {LayerType} END ===", context.LayerType);
            logger.Log(_options.LogLevel,
                "CorrelationId: {CorrelationId}, Duration: {Duration}ms",
                context.CorrelationId, duration.TotalMilliseconds);
        }
    }
}
