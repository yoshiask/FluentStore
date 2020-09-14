using AdGuard.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Notifications;

namespace FluentStore
{
    public static class Utils
    {
        public static readonly string[] INSTALLABLE_EXTS = new string[]
        {
            ".appx", ".appxbundle", ".msix", ".msixbundle"
        };

        public static async Task<bool> InstallPackage(Package package, ProductDetails product)
        {
            try
            {
                // Download the file to the app's temp directory
                //var client = new System.Net.WebClient();
                string filepath = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, package.Name);
                //client.DownloadFile(package.Uri, filepath);

                StorageFile destinationFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                    package.Name, CreationCollisionOption.ReplaceExisting);
                BackgroundDownloader downloader = new BackgroundDownloader();
                downloader.SuccessToastNotification = GenerateDonwloadSuccessToast(package, product);
                downloader.FailureToastNotification = GenerateDonwloadFailureToast(package, product);
                var progressToast = GenerateDonwloadProgressToast(package, product);
                DownloadOperation download = downloader.CreateDownload(package.Uri, destinationFile);
                download.RangesDownloaded += (op, args) =>
                {
                    ToastNotificationManager.GetDefault().CreateToastNotifier().Update(
                        new NotificationData(new Dictionary<string, string>()
                        {
                            { "progressValue", ((double)op.Progress.BytesReceived / op.Progress.TotalBytesToReceive).ToString() }
                        }),
                        progressToast.Tag
                    );
                };
                ToastNotificationManager.GetDefault().CreateToastNotifier().Show(progressToast);
                await download.StartAsync();

                // Clear the progress notif
                ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);

                // Pass the file to App Installer to install it
                Uri launchUri = new Uri("ms-appinstaller:?source=" + filepath);
                switch (await Launcher.QueryUriSupportAsync(launchUri, LaunchQuerySupportType.Uri))
                {
                    case LaunchQuerySupportStatus.Available:
                        return await Launcher.LaunchUriAsync(launchUri);

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static Package GetLatestDesktopPackage(List<Package> packages, ProductDetails product)
        {
            List<Package> installables = packages.FindAll(p => {
                return IsFiletype(p.Name, INSTALLABLE_EXTS) && (p.PackageFamily != null) && (p.PackageFamily == product.PackageFamilyNames?.First().Split("_")[0]);
            });
            if (installables.Count <= 0)
                return null;
            // TODO: Limit to only app packages, not dependencies
            Package latestPack = installables[0];
            foreach (Package p in installables.Skip(1))
            {
                if (p.Version > latestPack.Version)
                    latestPack = p;
            }
            return latestPack;
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

        public static ToastNotification GenerateDonwloadProgressToast(Package package, ProductDetails product)
        {
            var content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveProgressBar()
                            {
                                Value = new BindableProgressBarValue("progressValue"),
                                Title = new BindableString("progressTitle"),
                                Status = new BindableString("progressStatus")
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = product.Images.FindLast(i => i.ImageType == "logo").Url
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Pause", $"action=pauseDownload&packageName={package.Name}")
                        {
                            ActivationType = ToastActivationType.Background
                        },
                        new ToastButton("Cancel", $"action=cancelDownload&packageName={package.Name}")
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                },
                Launch = $"action=viewDownload&packageName={package.Name}"
            };

            var notif = new ToastNotification(content.GetXml());
            notif.Data = new NotificationData(new Dictionary<string, string>()
            {
                { "progressTitle", product.Title },
                { "progressStatus", "Downloading..." }
            });
            notif.Tag = package.PackageFamily;
            //notif.Group = "App Downloads";
            return notif;
        }
        
        public static ToastNotification GenerateDonwloadSuccessToast(Package package, ProductDetails product)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.Name}", ToastActivationType.Foreground)
                .AddText(product.Title)
                .AddText("Is ready to install")
                .AddAppLogoOverride(product.Images.FindLast(i => i.ImageType == "logo").Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }

        public static ToastNotification GenerateDonwloadFailureToast(Package package, ProductDetails product)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.Name}", ToastActivationType.Foreground)
                .AddText(product.Title)
                .AddText("Failed to download, please try again later")
                .AddAppLogoOverride(product.Images.FindLast(i => i.ImageType == "logo").Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }
    }
}
