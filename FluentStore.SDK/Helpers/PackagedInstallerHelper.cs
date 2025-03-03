using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.System;
using Windows.System.Profile;
using Windows.Foundation;
using FluentStore.SDK.PackageTypes;

namespace FluentStore.SDK.Helpers
{
    public static class PackagedInstallerHelper
    {
        /// <inheritdoc cref="PackageBase.GetCannotBeInstalledReason"/>
        public static string GetCannotBeInstalledReason(Stream stream, bool isBundle)
        {
            Guard.IsNotNull(stream, nameof(stream));

            // Open package archive for reading
            using var archive = new ZipArchive(stream);

            // Extract metadata from manifest
            List<ProcessorArchitecture> architectures = new();
            if (isBundle)
            {
                var bundleManifestEntry = archive.GetEntry("AppxMetadata/AppxBundleManifest.xml");
                using var bundleManifestStream = bundleManifestEntry.Open();
                XPathDocument bundleManifest = new(bundleManifestStream);
                var archNodes = bundleManifest.CreateNavigator().Select("//Package/@Architecture");
                do
                {
                    var archNode = archNodes.Current;
                    architectures.Add(Enum.Parse<ProcessorArchitecture>(archNode.Value, true));
                } while (archNodes.MoveNext());
            }
            else
            {
                var manifestEntry = archive.GetEntry("AppxManifest.xml");
                using var manifestStream = manifestEntry.Open();
                XPathDocument manifest = new(manifestStream);
                var archNode = manifest.CreateNavigator().SelectSingleNode("//Identity/@ProcessorArchitecture");
                architectures.Add(Enum.Parse<ProcessorArchitecture>(archNode.Value, true));
            }

            // Check Windows platform
            WindowsPlatform currentPlat = Extensions.ParseWindowsPlatform(AnalyticsInfo.VersionInfo.DeviceFamily);
            if (currentPlat == WindowsPlatform.Unknown)
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
                return "Package does not support " + curArch.ToString();
            }

            return null;
        }

        private static readonly Uri dummyUri = new("mailto:dummy@uwpcommunity.com");
        public static async Task<bool> IsInstalled(string packageFamilyName)
        {
            LaunchQuerySupportStatus result = await Launcher.QueryUriSupportAsync(dummyUri, LaunchQuerySupportType.Uri, packageFamilyName);
            return result switch
            {
                LaunchQuerySupportStatus.Available
                or LaunchQuerySupportStatus.NotSupported => true,
                _ => false,
            };
        }

        /// <inheritdoc cref="PackageBase.InstallAsync"/>
        public static async Task<bool> Install(PackageBase package)
        {
            try
            {
                PackageManager pkgManager = new();

                void InstallProgress(DeploymentProgress p)
                {
                    WeakReferenceMessenger.Default.Send(
                        new PackageInstallProgressMessage(package, p.percentage / 100));
                }

                // Deploy package
                WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(package));
                IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> operation;

                if (package.Type == InstallerType.AppInstaller)
                {
                    operation = pkgManager.AddPackageByAppInstallerFileAsync(
                        new Uri(package.DownloadItem.FullName),
                        AddPackageByAppInstallerOptions.ForceTargetAppShutdown,
                        pkgManager.GetDefaultPackageVolume());
                }
                else
                {
                    IEnumerable<Uri> dependencies = null;
                    if (package is IHasDependencies packageWithDependencies)
                        dependencies = packageWithDependencies.DependencyDownloadItems?.Select(f => new Uri(f.FullName)).ToArray();

                    operation = pkgManager.AddPackageAsync(
                        new Uri(package.DownloadItem.FullName),
                        dependencies,
                        DeploymentOptions.None);
                }

                var result = await operation.AsTask(new Progress<DeploymentProgress>(InstallProgress));
                if (!result.IsRegistered)
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(result.ExtendedErrorCode, package, ErrorType.PackageInstallFailed));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(package));
                    package.IsInstalled = true;
                }

                return result.IsRegistered;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, package, ErrorType.PackageInstallFailed));
                return false;
            }
        }

        /// <inheritdoc cref="PackageBase.LaunchAsync"/>
        public static async Task<bool> Launch(string packageFamilyName)
        {
            try
            {
                var pkgManager = new PackageManager();
                var pkg = pkgManager.FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();
                if (pkg == null) return false;

                var apps = await pkg.GetAppListEntriesAsync();
                if (apps.Count == 0) return false;

                return await apps[0].LaunchAsync();
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, type: ErrorType.PackageLaunchFailed));
                return false;
            }
        }

        public static InstallerType GetInstallerType(Stream stream)
        {
            InstallerType type = InstallerType.Unknown;

            var bytes = new byte[4];
            stream.Position = 0;
            stream.Read(bytes, 0, 4);
            uint magicNumber = (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);

            switch (magicNumber)
            {
                // ZIP
                /// Typical [not empty or spanned] ZIP archive
                case 0x504B0304:
                    using (ZipArchive archive = new(stream))
                    {
                        var entry = archive.GetEntry("[Content_Types].xml");
                        var ctypesXml = XDocument.Load(entry.Open());
                        var defaults = ctypesXml.Root.Elements().Where(e => e.Name.LocalName == "Default");
                        if (defaults.Any(d => d.Attribute("Extension").Value == "msix"))
                        {
                            // Package contains one or more MSIX packages
                            type |= InstallerType.Msix;
                        }
                        else if (defaults.Any(d => d.Attribute("Extension").Value == "appx"))
                        {
                            // Package contains one or more APPX packages
                            type |= InstallerType.AppX;
                        }
                        if (defaults.Any(d => d.Attribute("ContentType").Value == "application/vnd.ms-appx.bundlemanifest+xml"))
                        {
                            // Package is a bundle
                            type |= InstallerType.Bundle;
                        }

                        if (type == InstallerType.Unknown)
                        {
                            // We're not sure exactly what kind of package it is, but it's definitely
                            // a package archive. Even if it's not actually an appxbundle, it will
                            // likely still work.
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
                    type = InstallerType.EAppXBundle;
                    break;
            }

            return type;
        }

        /// <inheritdoc cref="PackageBase.CacheAppIcon"/>
        public static ImageBase GetAppIcon(Stream stream, bool isBundle)
        {
            // Open package archive for reading
            using var archive = new ZipArchive(stream);

            // Extract icon from manifest
            ZipArchive packArchive;
            if (isBundle)
            {
                // Get the smallest application APPX/MSIX
                var bundleManifestEntry = archive.GetEntry("AppxMetadata/AppxBundleManifest.xml");
                using var bundleManifestStream = bundleManifestEntry.Open();
                XmlDocument bundleManifest = new();
                bundleManifest.Load(bundleManifestStream);

                // Load namespace
                var defaultBundleNs = bundleManifest.DocumentElement.NamespaceURI;
                var bnsmgr = new XmlNamespaceManager(bundleManifest.NameTable);
                const string bd = "default";
                bnsmgr.AddNamespace(bd, defaultBundleNs);

                var packageNodes = bundleManifest.CreateNavigator().Select($"/{bd}:Bundle/{bd}:Packages/{bd}:Package[@Type=\"application\"]", bnsmgr);
                XPathNavigator smallestPackEntry = null;
                long smallestPackSize = long.MaxValue;
                do
                {
                    var packEntry = packageNodes.Current;
                    if (!long.TryParse(packEntry.GetAttribute("Size", string.Empty), out long packSize))
                        continue;
                    if (packSize < smallestPackSize)
                        smallestPackEntry = packEntry;
                } while (packageNodes.MoveNext());

                // Open the APPX/MSIX
                string filename = smallestPackEntry.GetAttribute("FileName", string.Empty);
                using Stream packStream = archive.GetEntry(Flurl.Url.Encode(filename)).Open();
                packArchive = new ZipArchive(packStream);
            }
            else
            {
                packArchive = archive;
            }

            // Get the app icon
            var manifestEntry = packArchive.GetEntry("AppxManifest.xml");
            using var manifestStream = manifestEntry.Open();
            XmlDocument manifest = new();
            manifest.Load(manifestStream);

            // Load namespace
            var defaultNs = manifest.DocumentElement.NamespaceURI;
            var nsmgr = new XmlNamespaceManager(manifest.NameTable);
            const string d = "default";
            nsmgr.AddNamespace(d, defaultNs);

            var logoNodes = manifest.CreateNavigator().Select($"/{d}:Package/{d}:Properties/{d}:Logo[1]", nsmgr);
            XPathNavigator logoNode = null;
            do
            {
                var cur = logoNodes.Current;
                if (cur.LocalName == "Logo")
                {
                    logoNode = cur;
                    break;
                }
            } while (logoNodes.MoveNext());

            // Find real entry using scaling
            ZipArchiveEntry iconEntry = packArchive.GetEntry(logoNode.Value);
            if (iconEntry == null)
            {
                string[] targetSplit = logoNode.Value.Replace('\\', '/').Split('.', 3);
                foreach (var entry in packArchive.Entries)
                {
                    string[] split = entry.FullName.Split('.', 3);
                    if (split.Length >= 3 && targetSplit[0] == split[0] && targetSplit[1] == split[2])
                    {
                        iconEntry = entry;
                        break;
                    }
                }
            }

            return new StreamImage
            {
                ImageType = ImageType.Logo,
                BackgroundColor = "Transparent",
                Stream = iconEntry.Open()
            };
        }

        public static string GetPackageFamilyName(Stream stream, bool isBundle)
        {
            Guard.IsNotNull(stream, nameof(stream));

            // Open package archive for reading
            using var archive = new ZipArchive(stream);

            // Extract metadata from manifest
            ZipArchiveEntry manifestEntry = archive.GetEntry(
                isBundle ? "AppxMetadata/AppxBundleManifest.xml" : "AppxManifest.xml");
            using var manifestStream = manifestEntry.Open();
            XmlDocument manifest = new();
            manifest.Load(manifestStream);

            // Load namespace
            var defaultNs = manifest.DocumentElement.NamespaceURI;
            var nsmgr = new XmlNamespaceManager(manifest.NameTable);
            const string d = "default";
            nsmgr.AddNamespace(d, defaultNs);

            var nameNode = manifest.SelectSingleNode($"//{d}:Identity/@Name", nsmgr);
            return nameNode.Value;
        }
    }
}
