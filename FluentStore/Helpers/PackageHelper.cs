using FluentStore.SDK;
using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Toolkit.Uwp.Notifications;
using StoreLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Notifications;

namespace FluentStore.Helpers
{
    public static class PackageHelper
    {
        public static PackageInstance GetLatestDesktopPackage(List<PackageInstance> packages, string family, ProductDetails product)
        {
            List<PackageInstance> installables = packages.FindAll(p => p.Version.Revision != 70);
            if (installables.Count <= 0)
                return null;
            // TODO: Add addtional checks that might take longer that the user can enable 
            // if they are having issues
            return installables.OrderByDescending(p => p.Version).First();
        }

        public static async Task<List<AppListEntry>> GetInstalledPackages()
        {
            PackageManager pkgManager = new PackageManager();
            var allEntries = await Task.WhenAll(pkgManager.FindPackagesForUser("")
                .Select(async pkg => await pkg.GetAppListEntriesAsync()));
            return allEntries.Select(e => e.FirstOrDefault()).Where(e => e != null).ToList();
        }

        public static async Task<AppListEntry> GetAppByPackageFamilyNameAsync(string packageFamilyName)
        {
            var pkgManager = new PackageManager();
            var pkg = pkgManager.FindPackagesForUser("", packageFamilyName).FirstOrDefault();

            if (pkg == null) return null;

            var apps = await pkg.GetAppListEntriesAsync();
            var firstApp = apps.FirstOrDefault();
            return firstApp;
        }

        public static bool IsFiletype(string file, params string[] exts)
        {
            foreach (string ext in exts)
            {
                if (Path.GetExtension(file) == ext)
                    return true;
            }
            return false;
        }

        private static readonly Uri dummyUri = new Uri("mailto:dummy@seznam.cz");
        /// <summary>
        /// Check if target <paramref name="packageName"/> is installed on this device.
        /// </summary>
        /// <param name="packageName">Package name in format: "949FFEAB.Email.cz_refxrrjvvv3cw"</param>
        /// <returns>True is app is installed on this device, false otherwise.</returns>
        public static async Task<bool> IsAppInstalledAsync(string packageName)
        {
            try
            {
                bool appInstalled;
                LaunchQuerySupportStatus result = await Launcher.QueryUriSupportAsync(dummyUri, LaunchQuerySupportType.Uri, packageName);
                switch (result)
                {
                    case LaunchQuerySupportStatus.Available:
                    case LaunchQuerySupportStatus.NotSupported:
                        appInstalled = true;
                        break;
                    //case LaunchQuerySupportStatus.AppNotInstalled:
                    //case LaunchQuerySupportStatus.AppUnavailable:
                    //case LaunchQuerySupportStatus.Unknown:
                    default:
                        appInstalled = false;
                        break;
                }

                Debug.WriteLine($"App {packageName}, query status: {result}, installed: {appInstalled}");
                return appInstalled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking if app {packageName} is installed. Error: {ex}");
                return false;
            }
        }

        public static ToastNotification GenerateProgressToast(PackageBase package)
        {
            var visualBinding = new ToastBindingGeneric
            {
                Children =
                {
                    new AdaptiveText
                    {
                        Text = new BindableString("progressTitle")
                    },
                    new AdaptiveProgressBar
                    {
                        Value = new BindableProgressBarValue("progressValue"),
                        Title = new BindableString("progressVersion"),
                        Status = new BindableString("progressStatus")
                    }
                },
            };
            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
            {
                visualBinding.AppLogoOverride = new ToastGenericAppLogo
                {
                    Source = image.Url
                };
            }

            var content = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = visualBinding
                },
                // TODO: Add cancel and pause functionality
                //Actions = new ToastActionsCustom()
                //{
                //    Buttons =
                //    {
                //        new ToastButton("Pause", $"action=pauseDownload&packageName={package.PackageMoniker}")
                //        {
                //            ActivationType = ToastActivationType.Background
                //        },
                //        new ToastButton("Cancel", $"action=cancelDownload&packageName={package.PackageMoniker}")
                //        {
                //            ActivationType = ToastActivationType.Background
                //        }
                //    }
                //},
                Launch = $"action=viewDownload&packageUrn={package.Urn}",
            };

            var notif = new ToastNotification(content.GetXml());
            notif.Data = new NotificationData(new Dictionary<string, string>()
            {
                { "progressTitle", package.Title },
                { "progressVersion", package.Version?.ToString() ?? string.Empty },
                { "progressStatus", "Downloading..." }
            });
            return notif;
        }

        public static ToastNotification GenerateDownloadSuccessToast(PackageBase package, StorageFile file)
        {
            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&packageUrn={package.Urn}&installerPath={file.Path}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText(package.Title + " is ready to install");

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static ToastNotification GenerateDownloadFailureToast(PackageBase package)
        {
            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&packageUrn={package.Urn}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText("Failed to download, please try again later");

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static ToastNotification GenerateInstallSuccessToast(PackageBase package)
        {
            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&packageUrn={package.Urn}", ToastActivationType.Foreground)
                .AddText(package.ShortTitle)
                .AddText(package.Title + " just got installed.");

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static ToastNotification GenerateInstallFailureToast(PackageBase package, Exception ex)
        {
            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&packageUrn={package.Urn}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText(package.Title + " failed to install.")
                .AddText(ex.Message);

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static void HandlePackageDownloadProgressToast(PackageDownloadProgressMessage m, ToastNotification progressToast)
        {
            ToastNotificationManager.GetDefault().CreateToastNotifier().Update(
                new NotificationData(new Dictionary<string, string>()
                {
                        { "progressValue", (m.Downloaded / m.Total).ToString() },
                        { "progressStatus", "Downloading..." }
                }),
                progressToast.Tag
            );
        }

        public static void HandlePackageDownloadStartedToast(PackageDownloadStartedMessage m, ToastNotification progressToast)
        {
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(progressToast);
        }

        public static void HandlePackageDownloadFailedToast(PackageDownloadFailedMessage m, ToastNotification progressToast)
        {
            // Hide progress notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateDownloadFailureToast(m.Package));
        }

        public static void HandlePackageInstallProgressToast(PackageInstallProgressMessage m, ToastNotification progressToast)
        {
            ToastNotificationManager.GetDefault().CreateToastNotifier().Update(
                new NotificationData(new Dictionary<string, string>()
                {
                    { "progressValue", m.Progress.ToString() },
                    { "progressStatus", "Installing..." }
                }),
                progressToast.Tag
            );
        }

        public static void HandlePackageInstallFailedToast(PackageInstallFailedMessage m, ToastNotification progressToast)
        {
            // Hide progress notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateInstallFailureToast(m.Package, m.Exception));
        }

        public static void HandlePackageInstallCompletedToast(PackageInstallCompletedMessage m, ToastNotification progressToast)
        {
            // Hide progress notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateInstallSuccessToast(m.Package));
        }

        public static string GetNotificationTag(Urn urn)
        {
            string tag = urn.ToString().Replace(':', '_');
            if (tag.Length > 64)
                tag = tag.Substring(0, 64);
            return tag;
        }
    }
}
