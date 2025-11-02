using FluentStore.SDK.Plugins.NuGet;
using System;
using System.Threading.Tasks;
using WinUIEx;

namespace FluentStore.Helpers.Updater;

internal static class AppUpdatePackageExtensions
{
    public static async Task<bool> CheckForUpdatesWithWindow(this AppUpdatePackageSource updater)
    {
        var update = await updater.GetPackage(AppUpdatePackageSource.FormatUrn(FluentStoreNuGetProject.CurrentSdkVersion.Release));

        if (update is not null && await update.CanInstallAsync())
        {
            Views.Update.UpdateWindow updateWindow = new(update);
            updateWindow.DispatcherQueue.TryEnqueue(delegate
            {
                updateWindow.CenterOnScreen();
                updateWindow.Activate();
            });

            return true;
        }

        return false;
    }
}
