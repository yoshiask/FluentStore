using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using WinGetRun;
using WinGetRun.Models;
using System.IO;

namespace FluentStore.SDK.Packages
{
    public class WinGetPackage : PackageBase<Package>
    {
        private readonly WinGetApi WinGetApi = Ioc.Default.GetRequiredService<WinGetApi>();

        public WinGetPackage(Package pack = null)
        {
            if (pack != null)
                Update(pack);
        }

        public void Update(Package pack)
        {
            Guard.IsNotNull(pack, nameof(pack));
            Model = pack;

            // Set base properties
            Title = pack.Latest.Name;
            PublisherId = pack.GetPublisherAndPackageIds().PublisherId;
            DeveloperName = pack.Latest.Publisher;
            ReleaseDate = pack.CreatedAt;
            Description = pack.Latest.Description;
            Version = pack.Versions[0];
            Website = pack.Latest.Homepage;

            // Set WinGet package properties
            PackageId = pack.GetPublisherAndPackageIds().PackageId;
        }

        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse("urn:" + Handlers.WinGetHandler.NAMESPACE_WINGET + ":" + Model.Id);
                return _Urn;
            }
            set => _Urn = value;
        }

        public override async Task<FileSystemInfo> DownloadPackageAsync(DirectoryInfo folder = null)
        {
            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            // Find the package URI
            if (!await PopulatePackageUri())
            {
                WeakReferenceMessenger.Default.Send(new PackageFetchFailedMessage(this, new Exception("An unknown error occurred.")));
                return null;
            }
            WeakReferenceMessenger.Default.Send(new PackageFetchCompletedMessage(this));

            // Download package
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);

            // Set the proper file name
            ((FileInfo)DownloadItem).MoveTo(Path.GetFileName(PackageUri.ToString()), true);

            WeakReferenceMessenger.Default.Send(new PackageDownloadCompletedMessage(this, (FileInfo)DownloadItem));
            Status = PackageStatus.Downloaded;
            return DownloadItem;
        }

        private async Task<bool> PopulatePackageUri()
        {
            Manifest = await WinGetApi.GetManifest(Urn.GetContent<NamespaceSpecificString>().UnEscapedValue, Version);
            PackageUri = new Uri(Manifest.Installers[0].Url);
            Website = Manifest.Homepage;

            Status = PackageStatus.DownloadReady;
            return true;
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            if (Model.IconUrl != null)
            {
                return new FileImage
                {
                    Url = Model.IconUrl,
                    ImageType = ImageType.Logo
                };
            }
            else
            {
                return TextImage.CreateFromName(Model.Latest.Name);
            }
        }

        public override Task<ImageBase> CacheHeroImage()
        {
            return null;
        }

        public override async Task<List<ImageBase>> CacheScreenshots()
        {
            return new List<ImageBase>();
        }

        public override async Task<bool> InstallAsync()
        {
            // Make sure installer is downloaded
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));
            bool isSuccess = false;

            // Get installer for current architecture
            var sysArch = Windows.ApplicationModel.Package.Current.Id.Architecture;
            Installer = Manifest.Installers.Find(i => sysArch == i.Arch.ToWinRTArch());
            if (Installer == null)
                Installer = Manifest.Installers.Find(i => i.Arch == WinGetRun.Enums.InstallerArchitecture.X86
                    || i.Arch == WinGetRun.Enums.InstallerArchitecture.Neutral);
            if (Installer == null)
            {
                string archStr = string.Join(", ", Manifest.Installers.Select(i => i.Arch));
                throw new PlatformNotSupportedException($"Your computer's architecture is {sysArch}, which is not supported by this package." +
                    $"This package supports {archStr}].");
            }

            switch (Installer.InstallerType ?? Manifest.InstallerType)
            {
                case WinGetRun.Enums.InstallerType.Appx:
                case WinGetRun.Enums.InstallerType.Msix:
                    isSuccess = await PackagedInstallerHelper.Install(this);
                    var file = (FileInfo)DownloadItem;
                    PackagedInstallerType = PackagedInstallerHelper.GetInstallerType(file);
                    PackageFamilyName = PackagedInstallerHelper.GetPackageFamilyName(file, PackagedInstallerType.Value.HasFlag(Models.InstallerType.Bundle));
                    break;

                default:
                    // TODO: Use full trust component to start installer in slient mode
                    var args = Installer.Switches?.Silent ?? Manifest.Switches?.Silent;
                    await Win32Helper.Install(this, args);
                    break;
            }

            if (isSuccess)
                Status = PackageStatus.Installed;
            return isSuccess;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            if (HasPackageFamilyName)
                return await PackagedInstallerHelper.IsInstalled(PackageFamilyName);

            return false;
        }

        public override async Task LaunchAsync()
        {
            switch (Installer.InstallerType ?? Manifest.InstallerType)
            {
                case WinGetRun.Enums.InstallerType.Appx:
                case WinGetRun.Enums.InstallerType.Msix:
                    Guard.IsTrue(HasPackageFamilyName, nameof(HasPackageFamilyName));
                    await PackagedInstallerHelper.Launch(PackageFamilyName);
                    break;
            }
        }

        private string _PackageFamilyName;
        public string PackageFamilyName
        {
            get => _PackageFamilyName;
            set => SetProperty(ref _PackageFamilyName, value);
        }
        public bool HasPackageFamilyName => PackageFamilyName != null;

        private Models.InstallerType? _PackagedInstallerType;
        public Models.InstallerType? PackagedInstallerType
        {
            get => _PackagedInstallerType;
            set => SetProperty(ref _PackagedInstallerType, value);
        }
        public bool HasPackagedInstallerType => PackagedInstallerType == null;

        private Uri _PackageUri;
        public Uri PackageUri
        {
            get => _PackageUri;
            set => SetProperty(ref _PackageUri, value);
        }

        private string _PackageId;
        public string PackageId
        {
            get => _PackageId;
            set => SetProperty(ref _PackageId, value);
        }

        private Manifest _Manifest;
        public Manifest Manifest
        {
            get => _Manifest;
            set => SetProperty(ref _Manifest, value);
        }

        private Installer _Installer;
        public Installer Installer
        {
            get => _Installer;
            set => SetProperty(ref _Installer, value);
        }
    }
}
