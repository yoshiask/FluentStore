using CommunityToolkit.Mvvm.ComponentModel;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.Sources;

public abstract partial class PluginPackageBase(PackageHandlerBase packageHandler, PluginLoader pluginLoader) : PackageBase(packageHandler)
{
    protected readonly PluginLoader _pluginLoader = pluginLoader;

    [ObservableProperty]
    private global::NuGet.Versioning.NuGetVersion _currentVersion;

    [ObservableProperty]
    private global::NuGet.Versioning.NuGetVersion _latestStableVersion;

    [ObservableProperty]
    private global::NuGet.Versioning.NuGetVersion _latestPrereleaseVersion;

    public override Task<ImageBase> CacheAppIcon() => Task.FromResult<ImageBase>(TextImage.CreateFromName(ShortTitle ?? Title));

    public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

    public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult<List<ImageBase>>([]);

    public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

    public override Task LaunchAsync()
    {
        throw new NotImplementedException();
    }

    public virtual async Task<bool> UninstallAsync()
    {
        var status = await _pluginLoader.UninstallPlugin(Urn.GetContent());
        return status.IsAtLeast(PluginInstallStatus.NoAction);
    }

    public virtual bool IsUpdateAvailable(bool includePrerelease = false)
    {
        // There isn't anything installed to update
        if (CurrentVersion is null)
            return false;

        return
            (includePrerelease && LatestPrereleaseVersion is not null && LatestPrereleaseVersion > CurrentVersion)
            || (LatestStableVersion > CurrentVersion);
    }

    public virtual ImageBase GetStatusImage()
    {
        // TODO: We don't really want the backend model to handle
        // UI stuff like this. Probably better to use a status enum,
        // maybe with flags.
        TextImage icon = new()
        {
            FontFamily = "Segoe MDL2 Assets",
        };
        
        if (IsUpdateAvailable())
        {
            icon.Text = "\uECC5";
            icon.ForegroundColor = SharedColors.Info;
        }
        else if (IsInstalled)
        {
            icon.Text = "\uE73E";
            icon.ForegroundColor = SharedColors.Success;
        }

        // TODO: Add status icons for plugins that are out-of-date

        return icon;
    }
}
