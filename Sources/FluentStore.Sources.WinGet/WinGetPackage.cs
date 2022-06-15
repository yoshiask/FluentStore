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
using Winstall;
using Winstall.Models;
using System.IO;
using FluentStore.SDK;
using FluentStore.SDK.Models;

namespace FluentStore.Sources.WinGet
{
    public class WinGetPackage : PackageBase<App>
    {
        private readonly WinstallApi _api;
        private string _appIcon;

        public WinGetPackage(PackageHandlerBase packageHandler, App pack = null, PopularApp popApp = null) : base(packageHandler)
        {
            if (packageHandler is not WinGetHandler winGetHandler)
                throw new ArgumentException($"Source handler must have an instance of {typeof(WinstallApi)}", nameof(packageHandler));
            _api = winGetHandler.GetApiClient();

            if (popApp != null)
                Update(popApp);
            if (pack != null)
                Update(pack);
        }

        public void Update(App pack)
        {
            Guard.IsNotNull(pack, nameof(pack));
            Model = pack;

            // Set base properties
            WinGetId = Model.Id;
            Urn = Urn.Parse($"urn:{WinGetHandler.NAMESPACE_WINGET}:{WinGetId}");
            Title = pack.Name;
            PublisherId = pack.GetPublisherAndPackageIds().PublisherId;
            DeveloperName = pack.Publisher;
            ReleaseDate = pack.UpdatedAt;
            Description = pack.Description;
            Version = pack.LatestVersion;
            Website = Link.Create(pack.HomepageUrl, ShortTitle + " website");

            // Set WinGet package properties
            PackageId = pack.GetPublisherAndPackageIds().PackageId;
            _appIcon = pack.GetImagePng();
        }

        public void Update(PopularApp popApp)
        {
            Guard.IsNotNull(popApp, nameof(popApp));

            // Set base properties
            Urn = Urn.Parse($"urn:{WinGetHandler.NAMESPACE_WINGET}:{popApp.Id}");
            WinGetId = popApp.Id;
            Title = popApp.Name;

            // Set WinGet package properties
            _appIcon = popApp.GetImagePng();
        }

        public void Update(WinGetRun.Models.Manifest manifest)
        {
            Guard.IsNotNull(manifest, nameof(manifest));
            Manifest = manifest;
            var installer = Manifest.Installers[0];

            PackageUri = new Uri(installer.Url);
            Website = Link.Create(Manifest.Homepage, ShortTitle + " website");

            if (installer.InstallerType.HasValue)
            {
                Type = installer.InstallerType.Value.ToSDKInstallerType();
            }
            else if (manifest.InstallerType.HasValue)
            {
                Type = manifest.InstallerType.Value.ToSDKInstallerType();
            }
            else if (Enum.TryParse<InstallerType>(Path.GetExtension(PackageUri.ToString())[1..], true, out var type))
            {
                Type = type;
            }
        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            // Find the package URI
            PackageUri = Model.Versions.First(av => av.Version == Model.LatestVersion).Installers[0].ToUri();
            Status = PackageStatus.DownloadReady;

            // Download package
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);
            if (!Status.IsAtLeast(PackageStatus.Downloaded))
                return null;

            // Set the proper file name
            DownloadItem = ((FileInfo)DownloadItem).CopyRename(Path.GetFileName(PackageUri.ToString()));

            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(this));
            Status = PackageStatus.Downloaded;
            return DownloadItem;
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            ImageBase icon = null;
            if (_appIcon != null)
            {
                icon = new FileImage
                {
                    Url = _appIcon,
                    ImageType = ImageType.Logo
                };
            }

            return icon ?? TextImage.CreateFromName(Title);
        }

        public override async Task<ImageBase> CacheHeroImage()
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
            Guard.IsTrue(Status.IsAtLeast(PackageStatus.Downloaded), nameof(Status));
            bool isSuccess = false;

            // Get installer for current architecture
            var sysArch = Win32Helper.GetSystemArchitecture();
            Installer = Manifest.Installers.Find(i => sysArch == i.Arch.ToSDKArch());
            if (Installer == null)
                Installer = Manifest.Installers.Find(i => i.Arch == WinGetRun.Enums.InstallerArchitecture.X86
                    || i.Arch == WinGetRun.Enums.InstallerArchitecture.Neutral);
            if (Installer == null)
            {
                string archStr = string.Join(", ", Manifest.Installers.Select(i => i.Arch));
                throw new PlatformNotSupportedException($"Your computer's architecture is {sysArch}, which is not supported by this package. " +
                    $"This package supports {archStr}.");
            }

            switch (Type.Reduce())
            {
                case InstallerType.Msix:
                    isSuccess = await PackagedInstallerHelper.Install(this);
                    var file = (FileInfo)DownloadItem;
                    PackagedInstallerType = PackagedInstallerHelper.GetInstallerType(file);
                    PackageFamilyName = PackagedInstallerHelper.GetPackageFamilyName(file, PackagedInstallerType.Value.HasFlag(InstallerType.Bundle));
                    break;

                default:
                    var args = Installer.Switches?.Silent ?? Manifest.Switches?.Silent;
                    isSuccess = await Win32Helper.Install(this, args);
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

        private string _WinGetId;
        public string WinGetId
        {
            get => _WinGetId;
            set => SetProperty(ref _WinGetId, value);
        }

        private string _PackageFamilyName;
        public string PackageFamilyName
        {
            get => _PackageFamilyName;
            set => SetProperty(ref _PackageFamilyName, value);
        }
        public bool HasPackageFamilyName => PackageFamilyName != null;

        private InstallerType? _PackagedInstallerType;
        public InstallerType? PackagedInstallerType
        {
            get => _PackagedInstallerType;
            set => SetProperty(ref _PackagedInstallerType, value);
        }
        public bool HasPackagedInstallerType => PackagedInstallerType == null;

        private string _PackageId;
        public string PackageId
        {
            get => _PackageId;
            set => SetProperty(ref _PackageId, value);
        }

        private WinGetRun.Models.Manifest _Manifest;
        public WinGetRun.Models.Manifest Manifest
        {
            get => _Manifest;
            set => SetProperty(ref _Manifest, value);
        }

        private WinGetRun.Models.Installer _Installer;
        public WinGetRun.Models.Installer Installer
        {
            get => _Installer;
            set => SetProperty(ref _Installer, value);
        }
    }
}
