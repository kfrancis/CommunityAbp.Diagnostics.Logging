using Volo.Abp.Application.Services;
using CommunityAbp.Diagnostics.Logging.Sample.Localization;

namespace CommunityAbp.Diagnostics.Logging.Sample.Services;

/* Inherit your application services from this class. */
public abstract class SampleAppService : ApplicationService
{
    protected SampleAppService()
    {
        LocalizationResource = typeof(SampleResource);
    }
}