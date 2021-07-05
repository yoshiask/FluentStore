using FluentStore.SDK.Handlers;
using FluentStore.SDK.Messages;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.System;
using Windows.System.Profile;

namespace FluentStore.SDK.Packages
{
    public class ModernPackage<TModel> : PackageBase<TModel>
    {
        [Flags]
        public enum InstallerType
        {
            Unknown     = 0b0000,
            AppX        = 0b0010,
            Msix        = 0b0100,
            Bundle      = 0b0001,

            AppXBundle  = AppX | Bundle,
            MsixBundle  = Msix | Bundle,
        }

        public override string HandlerId { get; set; } = nameof(MicrosoftStoreHandler);

        public override bool Equals(PackageBase other)
        {
            if (other is ModernPackage<TModel> mpackage)
            {
                return mpackage.Type == this.Type && mpackage.PackageFamilyName == this.PackageFamilyName
                    && mpackage.Version == this.Version;
            }
            else
            {
                return base.Equals(other);
            }
        }

        public override bool RequiresDownloadForCompatCheck => true;
        public override async Task<string> GetCannotBeInstalledReason()
        {
            Guard.IsNotNull(InstallerFile, nameof(InstallerFile));

            // Open package archive for reading
            using var stream = await InstallerFile.OpenReadAsync();
            //var reader = new BinaryReader(stream.AsStream());
            using var archive = new ZipArchive(stream.AsStream());

            // Extract metadata from manifest
            List<ProcessorArchitecture> architectures = new List<ProcessorArchitecture>();
            if (Type.HasFlag(InstallerType.Bundle))
            {
                var bundleManifestEntry = archive.GetEntry("AppxBundleManifest.xml");
                using var bundleManifestStream = bundleManifestEntry.Open();
                XPathDocument bundleManifest = new XPathDocument(bundleManifestStream);
                var archNodes = bundleManifest.CreateNavigator().Select("//Package/@Architecture");
                do
                {
                    var archNode = archNodes.Current;
                    architectures.Add((ProcessorArchitecture)Enum.Parse(typeof(ProcessorArchitecture), archNode.Value, true));
                } while (archNodes.MoveNext());
            }
            else
            {
                var manifestEntry = archive.GetEntry("AppxManifest.xml");
                using var manifestStream = manifestEntry.Open();
                XPathDocument manifest = new XPathDocument(manifestStream);
                var archNode = manifest.CreateNavigator().SelectSingleNode("//Identity/@ProcessorArchitecture");
                architectures.Add((ProcessorArchitecture)Enum.Parse(typeof(ProcessorArchitecture), archNode.Value, true));
            }

            // Check Windows platform
            PlatWindows? currentPlat = PlatWindowsStringConverter.Parse(AnalyticsInfo.VersionInfo.DeviceFamily);
            if (!currentPlat.HasValue)
            {
                return "Cannot identify the current Windows platform.";
            }
            //else if (!AllowedPlatforms.Contains(currentPlat.Value))
            //{
            //    return Title + " does not support " + currentPlat.ToString();
            //}

            // Check CPU architecture
            var curArch = Package.Current.Id.Architecture;
            if (!architectures.Contains(curArch))
            {
                return Title + " does not support " + curArch.ToString();
            }

            return null;
        }

        private static readonly Uri dummyUri = new Uri("mailto:dummy@uwpcommunity.com");
        public override async Task<bool> IsPackageInstalledAsync()
        {
            try
            {
                bool appInstalled;
                LaunchQuerySupportStatus result = await Launcher.QueryUriSupportAsync(dummyUri, LaunchQuerySupportType.Uri, PackageFamilyName);
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

                return appInstalled;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking if {PackageFamilyName} is installed. Error:\r\n{ex}");
                return false;
            }
        }

        public override async Task<bool> InstallAsync()
        {
            // Make sure installer is downloaded
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));

            bool isSuccess = true;
            PackageManager pkgManager = new PackageManager();
            Progress<DeploymentProgress> progressCallback = new Progress<DeploymentProgress>(prog =>
            {
                WeakReferenceMessenger.Default.Send(new PackageInstallProgressMessage(this, prog.percentage / 100));
            });

            WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(this));

            if (false)//Settings.Default.UseAppInstaller || (useAppInstaller.HasValue && useAppInstaller.Value))
            {
                // Pass the file to App Installer to install it
                Uri launchUri = new Uri("ms-appinstaller:?source=" + InstallerFile.Path);
                switch (await Launcher.QueryUriSupportAsync(launchUri, LaunchQuerySupportType.Uri))
                {
                    case LaunchQuerySupportStatus.Available:
                        isSuccess = await Launcher.LaunchUriAsync(launchUri);
                        if (!isSuccess)
                        {
                            WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(
                                this, new Exception("Failed to launch App Installer.")));
                            return false;
                            //finalNotif = GenerateInstallFailureToast(package, product, new Exception("Failed to launch App Installer."));
                        }
                        break;

                    case LaunchQuerySupportStatus.AppNotInstalled:
                        //finalNotif = GenerateInstallFailureToast(package, product, new Exception("App Installer is not available on this device."));
                        WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(
                                this, new Exception("App Installer is not available on this device.")));
                        return false;

                    case LaunchQuerySupportStatus.AppUnavailable:
                        //finalNotif = GenerateInstallFailureToast(package, product, new Exception("App Installer is not available right now, try again later."));
                        WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(
                                this, new Exception("App Installer is not available right now, try again later.")));
                        return false;

                    case LaunchQuerySupportStatus.Unknown:
                    default:
                        //finalNotif = GenerateInstallFailureToast(package, product, new Exception("An unknown error occured."));
                        WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(
                            this, new Exception("An unknown error occured.")));
                        return false;
                }
            }
            else
            {
                // Attempt to install the downloaded package
                var result = await pkgManager.AddPackageByUriAsync(
                    new Uri(InstallerFile.Path),
                    new AddPackageOptions()
                    {
                        ForceAppShutdown = true
                    }
                ).AsTask(progressCallback);

                if (!result.IsRegistered)
                {
                    WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(this, new Exception(result.ErrorText)));
                    return false;
                }
                isSuccess = result.IsRegistered;
                //await InstallerFile.DeleteAsync();
            }

            // Fire the success callback
            WeakReferenceMessenger.Default.Send(new PackageInstallCompletedMessage(this));

            Status = PackageStatus.Installed;
            return true;
        }

        public override Task<bool> DownloadPackageAsync(string installerPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the <see cref="InstallerType"/> of the downloaded package.
        /// Requires <see cref="PackageBase.Status"/> to be <see cref="PackageStatus.Downloaded"/>.
        /// </summary>
        /// <returns>The file extension that corresponds with the determined <see cref="InstallerType"/>.</returns>
        public async Task<string> GetInstallerType()
        {
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));
            string extension = "";
            InstallerType type = InstallerType.Unknown;

            string contentTypeinstallerPath = InstallerFile.Path + "_[Content_Types].xml";
            using (var stream = await InstallerFile.OpenStreamForReadAsync())
            {
                var bytes = new byte[4];
                stream.Read(bytes, 0, 4);
                uint magicNumber = (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);

                switch (magicNumber)
                {
                    // ZIP
                    /// Typical [not empty or spanned] ZIP archive
                    case 0x504B0304:
                        using (var archive = ZipFile.OpenRead(InstallerFile.Path))
                        {
                            var entry = archive.GetEntry("[Content_Types].xml");
                            entry.ExtractToFile(contentTypeinstallerPath, true);
                            var ctypesXml = XDocument.Load(contentTypeinstallerPath);
                            var defaults = ctypesXml.Root.Elements().Where(e => e.Name.LocalName == "Default");
                            if (defaults.Any(d => d.Attribute("Extension").Value == "msix"))
                            {
                                // Package contains one or more MSIX packages
                                extension += ".msix";
                                type |= InstallerType.Msix;
                            }
                            else if (defaults.Any(d => d.Attribute("Extension").Value == "appx"))
                            {
                                // Package contains one or more APPX packages
                                extension += ".appx";
                                type |= InstallerType.AppX;
                            }
                            if (defaults.Any(d => d.Attribute("ContentType").Value == "application/vnd.ms-appx.bundlemanifest+xml"))
                            {
                                // Package is a bundle
                                extension += "bundle";
                                type |= InstallerType.Bundle;
                            }

                            if (extension == string.Empty)
                            {
                                // We're not sure exactly what kind of package it is, but it's definitely
                                // a package archive. Even if it's not actually an appxbundle, it will
                                // likely still work.
                                extension = ".appxbundle";
                                type = InstallerType.AppXBundle;
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

            // Perform cleanup
            if (File.Exists(contentTypeinstallerPath))
                File.Delete(contentTypeinstallerPath);

            return extension;
        }

        private InstallerType _Type;
        public InstallerType Type
        {
            get => _Type;
            set => SetProperty(ref _Type, value);
        }

        private string _PackageFamilyName;
        public string PackageFamilyName
        {
            get => _PackageFamilyName;
            set => SetProperty(ref _PackageFamilyName, value);
        }

        private string _PublisherDisplayName;
        public string PublisherDisplayName
        {
            get => _PublisherDisplayName;
            set => SetProperty(ref _PublisherDisplayName, value);
        }

        private string _LogoRelativePath;
        public string LogoRelativePath
        {
            get => _LogoRelativePath;
            set => SetProperty(ref _LogoRelativePath, value);
        }
    }
}
