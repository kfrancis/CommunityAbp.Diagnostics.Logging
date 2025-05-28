using CommunityAbp.Diagnostics.Logging.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommunityAbp.Diagnostics.Logging.Extensions
{
    /// <summary>
    ///     Extension methods for configuring diagnostic logging
    /// </summary>
    public static class DiagnosticLoggingServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds diagnostic logging with default configuration
        /// </summary>
        public static IServiceCollection AddCommunityAbpDiagnosticsLogging(
            this IServiceCollection services)
        {
            return services.AddCommunityAbpDiagnosticsLogging(_ => { });
        }

        /// <summary>
        ///     Adds diagnostic logging with custom configuration
        /// </summary>
        public static IServiceCollection AddCommunityAbpDiagnosticsLogging(
            this IServiceCollection services,
            Action<DiagnosticOptions> configureOptions)
        {
            services.Configure(configureOptions);

            services.AddSingleton<DiagnosticOptions>(provider =>
            {
                var options = new DiagnosticOptions();
                configureOptions(options);
                return options;
            });

            return services;
        }

        /// <summary>
        ///     Adds diagnostic logging with configuration section binding
        /// </summary>
        public static IServiceCollection AddCommunityAbpDiagnosticsLogging(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = "CommunityAbp:Diagnostics:Logging")
        {
            services.Configure<DiagnosticOptions>(configuration.GetSection(sectionName));

            services.AddSingleton<DiagnosticOptions>(provider =>
            {
                var options = new DiagnosticOptions();
                configuration.GetSection(sectionName).Bind(options);
                return options;
            });

            return services;
        }

        /// <summary>
        ///     Adds diagnostic logging for development environment with enhanced logging
        /// </summary>
        public static IServiceCollection AddCommunityAbpDiagnosticsLoggingForDevelopment(
            this IServiceCollection services)
        {
            return services.AddCommunityAbpDiagnosticsLogging(options =>
            {
                options.EnableDiagnostics = true;
                options.RequireAttribute = false; // Log all methods in development
                options.LogStackTrace = true;
                options.LogLevel = LogLevel.Debug;
                options.LogArguments = true;
                options.LogUnitOfWork = true;
            });
        }

        /// <summary>
        ///     Adds diagnostic logging for production environment with minimal overhead
        /// </summary>
        public static IServiceCollection AddCommunityAbpDiagnosticsLoggingForProduction(
            this IServiceCollection services)
        {
            return services.AddCommunityAbpDiagnosticsLogging(options =>
            {
                options.EnableDiagnostics = true;
                options.RequireAttribute = true; // Only attributed methods
                options.LogStackTrace = false;
                options.LogLevel = LogLevel.Warning;
                options.LogArguments = false; // Avoid logging sensitive data
                options.LogUnitOfWork = true;
            });
        }
    }
}
