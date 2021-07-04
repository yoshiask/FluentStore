using FluentStore.SDK;
using FluentStore.SDK.Messages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using MicrosoftStore.Models;
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
        private static readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public static Action<ProductDetails> GettingPackagesCallback { get; set; }
        public static Action<ProductDetails> NoPackagesCallback { get; set; }
        public static Action<ProductDetails> PackagesLoadedCallback { get; set; }
        public static Action<ProductDetails, PackageInstance> PackageDownloadingCallback { get; set; }
        public static Action<ProductDetails, PackageInstance, ulong, ulong> PackageDownloadProgressCallback { get; set; }
        public static Action<ProductDetails, PackageInstance, StorageFile> PackageDownloadedCallback { get; set; }
        public static Action<ProductDetails, PackageInstance> PackageInstallingCallback { get; set; }
        public static Action<ProductDetails, PackageInstance> PackageInstalledCallback { get; set; }


        public static readonly string[] INSTALLABLE_EXTS = new string[]
        {
            ".appx", ".appxbundle", ".msix", ".msixbundle"
        };

        public static async Task<bool> InstallPackage(PackageInstance package, ProductDetails product, bool? useAppInstaller = null)
        {
            //ToastNotification finalNotif = GenerateInstallSuccessToast(package, product);
            bool isSuccess = true;
            try
            {
                (await DownloadPackage(package, product, false)).Deconstruct(out var installer, out var progressToast);

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

                PackageInstallingCallback?.Invoke(product, package);
                PackageInstallingCallback = null;

                if (Settings.Default.UseAppInstaller || (useAppInstaller.HasValue && useAppInstaller.Value))
                {
                    // Pass the file to App Installer to install it
                    Uri launchUri = new Uri("ms-appinstaller:?source=" + installer.Path);
                    switch (await Launcher.QueryUriSupportAsync(launchUri, LaunchQuerySupportType.Uri))
                    {
                        case LaunchQuerySupportStatus.Available:
                            isSuccess = await Launcher.LaunchUriAsync(launchUri);
                            //if (!isSuccess)
                            //    finalNotif = GenerateInstallFailureToast(package, product, new Exception("Failed to launch App Installer."));
                            break;

                        case LaunchQuerySupportStatus.AppNotInstalled:
                            //finalNotif = GenerateInstallFailureToast(package, product, new Exception("App Installer is not available on this device."));
                            isSuccess = false;
                            break;

                        case LaunchQuerySupportStatus.AppUnavailable:
                            //finalNotif = GenerateInstallFailureToast(package, product, new Exception("App Installer is not available right now, try again later."));
                            isSuccess = false;
                            break;

                        case LaunchQuerySupportStatus.Unknown:
                        default:
                            //finalNotif = GenerateInstallFailureToast(package, product, new Exception("An unknown error occured."));
                            isSuccess = false;
                            break;
                    }
                }
                else
                {
                    // Attempt to install the downloaded package
                    var result = await pkgManager.AddPackageByUriAsync(
                        new Uri(installer.Path),
                        new AddPackageOptions()
                        {
                            ForceAppShutdown = true
                        }
                    ).AsTask(progressCallback);

                    //if (result.IsRegistered)
                    //    finalNotif = GenerateInstallSuccessToast(package, product);
                    //else
                    //    finalNotif = GenerateInstallFailureToast(package, product, result.ExtendedErrorCode);
                    isSuccess = result.IsRegistered;
                    await installer.DeleteAsync();
                }

                // Hide progress notification
                //ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);
                // Show the final notification
                //ToastNotificationManager.GetDefault().CreateToastNotifier().Show(finalNotif);
                // Fire the success callback
                PackageInstalledCallback?.Invoke(product, package);
                PackageInstalledCallback = null;

                return true;
            }
            catch (Exception ex)
            {
                //ToastNotificationManager.GetDefault().CreateToastNotifier().Show(GenerateInstallFailureToast(package, product, ex));
                return false;
            }
        }

        public static async Task<bool> InstallPackage(ProductDetails product, bool? useAppInstaller = null)
        {
            var culture = CultureInfo.CurrentUICulture;

            GettingPackagesCallback?.Invoke(product);
            GettingPackagesCallback = null;

            var dcathandler = new StoreLib.Services.DisplayCatalogHandler(DCatEndpoint.Production, new StoreLib.Services.Locale(culture, true));
            await dcathandler.QueryDCATAsync(product.ProductId);
            var packs = await dcathandler.GetMainPackagesForProductAsync();
            string packageFamilyName = dcathandler.ProductListing.Product.Properties.PackageFamilyName;

            PackagesLoadedCallback?.Invoke(product);
            PackagesLoadedCallback = null;

            if (packs != null)
            {
                var package = GetLatestDesktopPackage(packs.ToList(), packageFamilyName, product);
                if (package == null)
                {
                    NoPackagesCallback?.Invoke(product);
                    NoPackagesCallback = null;
                    return false;
                }
                else
                {
                    if (await InstallPackage(package, product, useAppInstaller))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> DownloadPackageAsync(ProductDetails product, string installerPath = null)
        {
            // Download the file to the app's temp directory
            if (installerPath == null)
                installerPath = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, product.PackageFamilyNames.First() + "_" + product.Version);
            return await new SDK.Packages.MicrosoftStorePackage(product).DownloadPackageAsync(installerPath);
        }

        public static async Task<Tuple<StorageFile, ToastNotification>> DownloadPackage(PackageInstance package, ProductDetails product, bool hideProgressToastWhenDone = true, string filepath = null)
        {
            // Download the file to the app's temp directory
            if (filepath == null)
                filepath = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, package.PackageMoniker);
            Debug.WriteLine(filepath);

            StorageFile destinationFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                package.PackageMoniker, CreationCollisionOption.ReplaceExisting);
            BackgroundDownloader downloader = new BackgroundDownloader();
            //downloader.FailureToastNotification = GenerateDownloadFailureToast(package, product);
            //var progressToast = GenerateProgressToast(package, product);
            Debug.WriteLine(package.PackageUri.AbsoluteUri);
            DownloadOperation download = downloader.CreateDownload(package.PackageUri, destinationFile);
            download.RangesDownloaded += (op, args) =>
            {
                //ToastNotificationManager.GetDefault().CreateToastNotifier().Update(
                //    new NotificationData(new Dictionary<string, string>()
                //    {
                //        { "progressValue", ((double)op.Progress.BytesReceived / op.Progress.TotalBytesToReceive).ToString() },
                //        { "progressStatus", "Downloading..." }
                //    }),
                //    progressToast.Tag
                //);

                PackageDownloadProgressCallback?.Invoke(product, package, op.Progress.BytesReceived, op.Progress.TotalBytesToReceive);
            };

            PackageDownloadingCallback?.Invoke(product, package);
            PackageDownloadingCallback = null;
            //ToastNotificationManager.GetDefault().CreateToastNotifier().Show(progressToast);

            await download.StartAsync();

            string extension = "";
            string contentTypeFilepath = filepath + "_[Content_Types].xml";
            using (var stream = await destinationFile.OpenStreamForReadAsync())
            {
                var bytes = new byte[4];
                stream.Read(bytes, 0, 4);
                uint magicNumber = (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);

                switch (magicNumber)
                {
                    // ZIP
                    /// Typical [not empty or spanned] ZIP archive
                    case 0x504B0304:
                        using (var archive = ZipFile.OpenRead(filepath))
                        {
                            var entry = archive.GetEntry("[Content_Types].xml");
                            entry.ExtractToFile(contentTypeFilepath, true);
                            var ctypesXml = XDocument.Load(contentTypeFilepath);
                            var defaults = ctypesXml.Root.Elements().Where(e => e.Name.LocalName == "Default");
                            if (defaults.Any(d => d.Attribute("Extension").Value == "msix"))
                            {
                                // Package contains one or more MSIX packages
                                extension += ".msix";
                            }
                            else if (defaults.Any(d => d.Attribute("Extension").Value == "appx"))
                            {
                                // Package contains one or more APPX packages
                                extension += ".appx";
                            }
                            if (defaults.Any(defaults => defaults.Attribute("ContentType").Value == "application/vnd.ms-appx.bundlemanifest+xml"))
                            {
                                // Package is a bundle
                                extension += "bundle";
                            }

                            if (extension == string.Empty)
                            {
                                // We're not sure exactly what kind of package it is, but it's definitely
                                // a package archive. Even if it's not actually an appxbundle, it will
                                // likely still work.
                                extension = ".appxbundle";
                            }
                        }
                        break;

                    // EMSIX, EAAPX, EMSIXBUNDLE, EAPPXBUNDLE
                    /// An encrypted installer [bundle]?
                    case 0x45584248:
                        // This means the downloaded file wasn't a zip archive.
                        // Some inspection of a hex dump of the file leads me to believe that this means
                        // the installer is encrypted. There's probably nothing that can be done about this,
                        // but since it's a known case, let's leave this here.
                        extension = ".eappxbundle";
                        break;
                }
            }

            if (File.Exists(contentTypeFilepath))
                File.Delete(contentTypeFilepath);

            if (extension != string.Empty)
                await destinationFile.RenameAsync(destinationFile.Name + extension, NameCollisionOption.ReplaceExisting);

            PackageDownloadProgressCallback = null;
            PackageDownloadedCallback?.Invoke(product, package, destinationFile);
            PackageDownloadedCallback = null;
            //if (hideProgressToastWhenDone)
            //    ToastNotificationManager.GetDefault().CreateToastNotifier().Hide(progressToast);

            return (destinationFile, (ToastNotification)null).ToTuple();
        }

        public static async Task<Tuple<StorageFile, ToastNotification>> DownloadPackage(ProductDetails product,
            bool hideProgressToastWhenDone = true, string filepath = null)
        {
            var culture = CultureInfo.CurrentUICulture;

            GettingPackagesCallback?.Invoke(product);
            GettingPackagesCallback = null;

            var dcathandler = new StoreLib.Services.DisplayCatalogHandler(DCatEndpoint.Production, new StoreLib.Services.Locale(culture, true));
            await dcathandler.QueryDCATAsync(product.ProductId);
            var packs = await dcathandler.GetMainPackagesForProductAsync();
            string packageFamilyName = dcathandler.ProductListing.Product.Properties.PackageFamilyName;

            PackagesLoadedCallback?.Invoke(product);
            PackagesLoadedCallback = null;

            if (packs != null)
            {
                var package = GetLatestDesktopPackage(packs.ToList(), packageFamilyName, product);
                if (package == null)
                {
                    NoPackagesCallback?.Invoke(product);
                    NoPackagesCallback = null;
                    return null;
                }
                else
                {
                    var result = await DownloadPackage(package, product, hideProgressToastWhenDone, filepath);
                    if (result != null && result.Item1 != null)
                    {
                        PackageDownloadedCallback?.Invoke(product, package, result.Item1);
                        PackageDownloadedCallback = null;
                        return result;
                    }
                }
            }

            return null;
        }

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
                            Source = package.Images.FindLast(i => i.ImageType == ImageType.Logo).Url
                        }
                    }
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
                Launch = $"action=viewDownload&packageName={package.PackageId}",
            };

            var notif = new ToastNotification(content.GetXml());
            notif.Data = new NotificationData(new Dictionary<string, string>()
            {
                { "progressTitle", package.Title },
                { "progressVersion", package.Version.ToString() },
                { "progressStatus", "Downloading..." }
            });
            notif.Tag = package.PackageId;
            //notif.Group = "App Downloads";
            return notif;
        }

        public static ToastNotification GenerateDownloadSuccessToast(PackageBase package, StorageFile file)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.PackageId}&installerPath={file.Path}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText(package.Title + " is ready to install")
                .AddAppLogoOverride(package.Images.FindLast(i => i.ImageType == ImageType.Logo).Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }

        public static ToastNotification GenerateDownloadFailureToast(PackageBase package)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.PackageId}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText("Failed to download, please try again later")
                .AddAppLogoOverride(package.Images.FindLast(i => i.ImageType == ImageType.Logo).Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }

        public static ToastNotification GenerateInstallSuccessToast(PackageBase package)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.PackageId}", ToastActivationType.Foreground)
                .AddText(package.ShortTitle)
                .AddText(package.Title + " just got installed.")
                .AddAppLogoOverride(package.Images.FindLast(i => i.ImageType == ImageType.Logo).Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
        }

        public static ToastNotification GenerateInstallFailureToast(PackageBase package, Exception ex)
        {
            var content = new ToastContentBuilder().SetToastScenario(ToastScenario.Reminder)
                .AddToastActivationInfo($"action=viewEvent&eventId={package.PackageId}", ToastActivationType.Foreground)
                .AddText(package.Title)
                .AddText(package.Title + " failed to install.")
                .AddText(ex.Message)
                .AddAppLogoOverride(package.Images.FindLast(i => i.ImageType == ImageType.Logo).Uri, addImageQuery: false)
                .Content;
            return new ToastNotification(content.GetXml());
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
    }
}
