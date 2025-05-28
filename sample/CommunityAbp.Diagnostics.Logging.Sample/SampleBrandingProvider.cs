using Microsoft.Extensions.Localization;
using CommunityAbp.Diagnostics.Logging.Sample.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace CommunityAbp.Diagnostics.Logging.Sample;

[Dependency(ReplaceServices = true)]
public class SampleBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<SampleResource> _localizer;

    public SampleBrandingProvider(IStringLocalizer<SampleResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}