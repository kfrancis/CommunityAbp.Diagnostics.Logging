using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace CommunityAbp.Diagnostics.Logging.Interceptors
{
    /// <summary>
    ///     Used to register the ApplicationServiceDiagnosticsInterceptor for all application services, if applicable.
    /// </summary>
    public static class ApplicationServiceDiagnosticsInterceptorRegistrar
    {
        /// <summary>
        ///     Registers the ApplicationServiceDiagnosticsInterceptor if the service type should be intercepted.
        /// </summary>
        /// <param name="context">
        ///     The context containing information about the service being registered, including its type and interceptors.
        /// </param>
        public static void RegisterIfNeeded(IOnServiceRegistredContext context)
        {
            if (ShouldIntercept(context.ImplementationType))
                context.Interceptors.TryAdd<ApplicationServiceDiagnosticsInterceptor>();
        }

        /// <summary>
        ///     Determines if the given type should be intercepted by the ApplicationServiceDiagnosticsInterceptor.
        /// </summary>
        /// <param name="type">
        ///     The type to check for interception. If null, returns false.
        /// </param>
        /// <returns>
        ///     True if the type should be intercepted, false otherwise. This includes checks for whether the type is an
        ///     application service,
        /// </returns>
        private static bool ShouldIntercept(Type? type)
        {
            if (type == null) return false;

            // Avoid ABP's ignored types
            if (DynamicProxyIgnoreTypes.Contains(type)) return false;

            // Only intercept Application Services
            if (!typeof(IApplicationService).IsAssignableFrom(type)) return false;

            // Avoid abstract classes and interfaces
            if (type.IsAbstract || type.IsInterface) return false;

            // Avoid system/framework types
            if (type.Namespace?.StartsWith("Microsoft.") == true ||
                type.Namespace?.StartsWith("System.") == true ||
                type.Namespace?.StartsWith("Volo.Abp.") == true)
                return false;

            return true;
        }
    }
}