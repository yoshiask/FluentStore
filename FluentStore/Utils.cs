using AdGuard.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
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
                //downloader.SuccessToastNotification = GenerateDownloadSuccessToast(package, product);
                downloader.FailureToastNotification = GenerateDownloadFailureToast(package, product);
                var progressToast = GenerateProgressToast(package, product);
                DownloadOperation download = downloader.CreateDownload(package.Uri, destinationFile);
                download.RangesDownloaded += (op, args) =>
                {
                    ToastNotificationManager.GetDefault().CreateToastNotifier().Update(
                        new NotificationData(new Dictionary<string, string>()
                        {
                            { "progressValue", ((double)op.Progress.BytesReceived / op.Progress.TotalBytesToReceive).ToString() },
                            { "progressStatus", "Downloading..." }
                        }),
                        progressToast.Tag
                    );
                };
                ToastNotificationManager.GetDefault().CreateToastNotifier().Show(progressToast);
                await download.StartAsync();

                PackageManager pkgManager = new PackageManager();
                Progress<DeploymentProgress> progressCallback = new Progress<DeploymentProgress>(prog =>
                {
                    ToastNotificationManager.GetDefault().CreateToastNotifier().Update(
                        new NotificationData(new Dictionary<string, string>()
                        {
                            { "progressValue", (prog.percentage / 100).ToString() },
                            { "progressStatus", "Installing..." }
                        }),
                        progressToast.Tag
                    );
                });

                // Attempt to install the downloaded package
                var result = await pkgManager.AddPackageAsync(new Uri(filepath), new Uri[] { }, DeploymentOptions.ForceTargetApplicationShutdown).AsTask(progressCallback);
                
                if (result.IsRegistered)
				{
                    // Remove the progress notifcation
                    ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);

                    // Show the success notification
                    ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateInstallSuccessToast(package, product));
                    return true;
                }
                else
				{
                    return false;
				}

                // Pass the file to App Installer to install it
                //Uri launchUri = new Uri("ms-appinstaller:?source=" + filepath);
                //switch (await Launcher.QueryUriSupportAsync(launchUri, LaunchQuerySupportType.Uri))
                //{
                //    case LaunchQuerySupportStatus.Available:
                //        return await Launcher.LaunchUriAsync(launchUri);

                //    default:
                //        return false;
                //}
            }
            catch
            {
                return false;
            }
        }

        public static Package GetLatestDesktopPackage(List<Package> packages, ProductDetails product)
        {
            List<Package> installables = packages.FindAll(p => {
                return IsFiletype(p.Name, INSTALLABLE_EXTS)
                    && (p.PackageFamily != null)
                    && (p.PackageFamily == product.PackageFamilyNames?.First().Split("_")[0])
                    && ((p.Architecture == "neutral") || (p.Architecture == "x64"));
            });
            if (installables.Count <= 0)
                return null;
            // TODO: Add addtional checks that might take longer that the user can enable 
            // if they are having issues
            return installables.OrderByDescending(p => p.Version).First();
            // TODO: Limit to only app packages, not dependencies
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

		public static ToastNotification GenerateProgressToast(Package package, ProductDetails product)
        {
            var content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
							{
                                Text = new BindableString("progressTitle")
                            },
                            new AdaptiveProgressBar()
                            {
                                Value = new BindableProgressBarValue("progressValue"),
                                Title = new BindableString("progressVersion"),
                                Status = new BindableString("progressStatus")
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = product.Images.FindLast(i => i.ImageType == "logo").Url
                        }
                    }
                },
                // TODO: Add cancel and pause functionality
                //Actions = new ToastActionsCustom()
                //{
                //    Buttons =
                //    {
                //        new ToastButton("Pause", $"action=pauseDownload&packageName={package.Name}")
                //        {
                //            ActivationType = ToastActivationType.Background
                //        },
                //        new ToastButton("Cancel", $"action=cancelDownload&packageName={package.Name}")
                //        {
                //            ActivationType = ToastActivationType.Background
                //        }
                //    }
                //},
                Launch = $"action=viewDownload&packageName={package.Name}",
            };

            var notif = new ToastNotification(content.GetXml());
            notif.Data = new NotificationData(new Dictionary<string, string>()
            {
                { "progressTitle", product.Title },
                { "progressVersion", package.Version.ToString() },
                { "progressStatus", "Downloading..." }
            });
            notif.Tag = package.PackageFamily;
            //notif.Group = "App Downloads";
            return notif;
        }
        
        public static ToastNotification GenerateDownloadSuccessToast(Package package, ProductDetails product)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.Name}", ToastActivationType.Foreground)
                .AddText(product.Title)
                .AddText(product.Title + " is ready to install")
                .AddAppLogoOverride(product.Images.FindLast(i => i.ImageType == "logo").Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }

        public static ToastNotification GenerateDownloadFailureToast(Package package, ProductDetails product)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.Name}", ToastActivationType.Foreground)
                .AddText(product.Title)
                .AddText("Failed to download, please try again later")
                .AddAppLogoOverride(product.Images.FindLast(i => i.ImageType == "logo").Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }

        public static ToastNotification GenerateInstallSuccessToast(Package package, ProductDetails product)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.Name}", ToastActivationType.Foreground)
                .AddText(product.Title)
                .AddText(product.Title + " just got installed.")
                .AddAppLogoOverride(product.Images.FindLast(i => i.ImageType == "logo").Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }
    }
}
