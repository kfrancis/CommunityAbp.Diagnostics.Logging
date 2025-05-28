using CommunityAbp.Diagnostics.Logging.Configuration;
using CommunityAbp.Diagnostics.Logging.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace CommunityAbp.Diagnostics.Logging
{
    /// <summary>
    ///     CommunityAbp Diagnostics Logging Module
    /// </summary>
    public class CommunityAbpDiagnosticsLoggingModule : AbpModule
    {
        /// <inheritdoc />
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            // Register diagnostic options with configuration binding
            context.Services.Configure<DiagnosticOptions>(
                configuration.GetSection("CommunityAbp:Diagnostics:Logging")
            );

            // Register as singleton for injection into interceptor
            context.Services.AddSingleton<DiagnosticOptions>(provider =>
            {
                var options = new DiagnosticOptions();
                configuration.GetSection("CommunityAbp:Diagnostics:Logging").Bind(options);
                return options;
            });
        }

        /// <inheritdoc />
        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            // Auto-register interceptor for application services
            context.Services.OnRegistered(ApplicationServiceDiagnosticsInterceptorRegistrar.RegisterIfNeeded);
        }
    }
}