using FluentStore.SDK;
using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.WinUI.Notifications;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Notifications;

namespace FluentStore.Helpers
{
    public static class PackageHelper
    {
        public static ToastNotification GenerateProgressToast(PackageBase package)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return null;

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
                Launch = $"package/{package.Urn}",
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

        public static ToastNotification GenerateDownloadSuccessToast(PackageBase package)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return null;

            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"package/{package.Urn}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText(package.Title + " is ready to install");

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static ToastNotification GenerateDownloadFailureToast(PackageBase package)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return null;

            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"package/{package.Urn}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText("Failed to download, please try again later");

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static ToastNotification GenerateInstallSuccessToast(PackageBase package)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return null;

            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"package/{package.Urn}", ToastActivationType.Foreground)
                .AddText(package.ShortTitle)
                .AddText(package.Title + " just got installed.");

            try
            {
                if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                    builder.AddAppLogoOverride(image.Uri, addImageQuery: false);
            } finally { }

            return new ToastNotification(builder.GetXml());
        }

        public static ToastNotification GenerateInstallFailureToast(PackageBase package, Exception ex)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return null;

            var builder = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"/package/{package.Urn}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText(package.Title + " failed to install.")
                .AddText(ex.Message);

            if (package.GetAppIcon().Result is SDK.Images.FileImage image && !image.Uri.IsFile)
                builder.AddAppLogoOverride(image.Uri, addImageQuery: false);

            return new ToastNotification(builder.GetXml());
        }

        public static void HandlePackageDownloadProgressToast(PackageDownloadProgressMessage m, ToastNotification progressToast)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

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
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(progressToast);
        }

        public static void HandlePackageDownloadFailedToast(PackageDownloadFailedMessage m, ToastNotification progressToast)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

            // Hide progress notification
            Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateDownloadFailureToast(m.Package));
        }

        public static void HandlePackageDownloadCompletedToast(PackageDownloadCompletedMessage m, ToastNotification progressToast)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

            // Hide progress notification
            Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateDownloadSuccessToast(m.Package));
        }

        public static void HandlePackageInstallProgressToast(PackageInstallProgressMessage m, ToastNotification progressToast)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

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
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

            // Hide progress notification
            Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateInstallFailureToast(m.Package, m.Exception));
        }

        public static void HandlePackageInstallCompletedToast(PackageInstallCompletedMessage m, ToastNotification progressToast)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
                return;

            // Hide progress notification
            Hide(progressToast);
            // Show the final notification
            ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateInstallSuccessToast(m.Package));
        }

        private static void Hide(ToastNotification toast)
        {
            try
            {
                ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(toast);
            }
            catch { }
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
