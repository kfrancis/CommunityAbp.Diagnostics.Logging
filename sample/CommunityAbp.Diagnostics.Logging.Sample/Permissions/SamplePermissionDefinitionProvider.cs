using CommunityAbp.Diagnostics.Logging.Sample.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace CommunityAbp.Diagnostics.Logging.Sample.Permissions;

public class SamplePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SamplePermissions.GroupName);


        myGroup.AddPermission(SamplePermissions.Dashboard.Host, L("Permission:Dashboard"), MultiTenancySides.Host);

        var booksPermission = myGroup.AddPermission(SamplePermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(SamplePermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(SamplePermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(SamplePermissions.Books.Delete, L("Permission:Books.Delete"));
        
        //Define your own permissions here. Example:
        //myGroup.AddPermission(SamplePermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SampleResource>(name);
    }
}
