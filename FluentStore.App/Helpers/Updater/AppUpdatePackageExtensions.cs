using FluentStore.SDK.Plugins.NuGet;
using System;
using System.Threading.Tasks;
using WinUIEx;

namespace FluentStore.Helpers.Updater;

internal static class AppUpdatePackageExtensions
{
    public static async Task<UpdateCheckResult> CheckForUpdatesWithWindow(this AppUpdatePackageSource updater)
    {
        try
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

                return UpdateCheckResult.UpdateAvailable;
            }

            return UpdateCheckResult.NoUpdateAvailable;
        }
        catch
        {
            return UpdateCheckResult.Error;
        }
    }
}

internal enum UpdateCheckResult
{
    Error,
    NoUpdateAvailable,
    UpdateAvailable,
}
