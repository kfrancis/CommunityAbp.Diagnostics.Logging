using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace CommunityAbp.Diagnostics.Logging.Sample.Data;

public class SampleDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public SampleDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the SampleDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SampleDbContext>()
            .Database
            .MigrateAsync();

    }
}
