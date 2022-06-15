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
using Winstall.Models.Manifest;
using Winstall.Models.Manifest.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

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
            _appIcon ??= pack.GetImagePng();
        }

        public void Update(PopularApp popApp)
        {
            Guard.IsNotNull(popApp, nameof(popApp));

            // Set base properties
            Urn = Urn.Parse($"urn:{WinGetHandler.NAMESPACE_WINGET}:{popApp.Id}");
            WinGetId = popApp.Id;
            Title = popApp.Name;

            // Set WinGet package properties
            _appIcon ??= popApp.GetImagePng();
        }

        public void Update(Locale locale)
        {
            Guard.IsNotNull(locale, nameof(locale));

            // Set base properties
            WinGetId ??= locale.PackageIdentifier;
            Urn ??= Urn.Parse($"urn:{WinGetHandler.NAMESPACE_WINGET}:{WinGetId}");
            Title = locale.PackageName;
            PublisherId = locale.PackageIdentifier.Split(new[] { '.' }, 2)[0];
            DeveloperName = locale.Publisher;
            Description = locale.Description;
            Version = locale.PackageVersion;
            Website = Link.Create(locale.PackageUrl, ShortTitle + " website");
            PrivacyUri = Link.Create(locale.PrivacyUrl, ShortTitle + " privacy policy");

            // Set WinGet properties
            SupportUrl = Link.Create(locale.PublisherSupportUrl, DeveloperName + " support");
        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            if (!Status.IsAtLeast(PackageStatus.BasicDetails))
                return null;

            // Find the package URI
            await PopulatePackageUri();
            if (!Status.IsAtLeast(PackageStatus.DownloadReady))
                return null;

            // Download package
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);
            if (!Status.IsAtLeast(PackageStatus.Downloaded))
                return null;

            // Set the proper file name
            DownloadItem = ((FileInfo)DownloadItem).CopyRename(Path.GetFileName(PackageUri.ToString()));

            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(this));
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

            switch (Type.Reduce())
            {
                case InstallerType.Msix:
                    isSuccess = await PackagedInstallerHelper.Install(this);
                    var file = (FileInfo)DownloadItem;
                    Type = PackagedInstallerHelper.GetInstallerType(file);
                    PackageFamilyName = PackagedInstallerHelper.GetPackageFamilyName(file, Type.HasFlag(InstallerType.Bundle));
                    break;

                default:
                    var args = Installer.InstallerSwitches?.Silent ?? Manifest.InstallerSwitches?.Silent;
                    var successCodes = Installer.InstallerSuccessCodes ?? Manifest.InstallerSuccessCodes;
                    var errorCodes = Installer.ExpectedReturnCodes ?? Manifest.ExpectedReturnCodes;

                    isSuccess = await Win32Helper.Install(this, args,
                        successCodes, errorCodes?.Select(ec => ec.InstallerReturnCode),
                        GetInstallerErrorMessage);
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
            switch (Type.Reduce())
            {
                case InstallerType.Msix:
                    Guard.IsTrue(HasPackageFamilyName, nameof(HasPackageFamilyName));
                    await PackagedInstallerHelper.Launch(PackageFamilyName);
                    break;
            }
        }

        private async Task PopulatePackageUri()
        {
            Manifest = await CommunityRepo.GetInstallerAsync(WinGetId, Version);

            // Get installer for current architecture
            var sysArch = Win32Helper.GetSystemArchitecture();
            Installer = Manifest.Installers.Find(i => sysArch == i.Architecture.ToSDKArch());
            if (Installer == null)
                Installer = Manifest.Installers.Find(i => i.Architecture == InstallerArchitecture.X86
                    || i.Architecture == InstallerArchitecture.Neutral);
            if (Installer == null)
            {
                string archStr = string.Join(", ", Manifest.Installers.Select(i => i.Architecture));
                throw new PlatformNotSupportedException($"Your computer's architecture is {sysArch}, which is not supported by this package. " +
                    $"This package supports {archStr}.");
            }

            Type = Installer.InstallerType?.ToSDKInstallerType()
                ?? Manifest.InstallerType?.ToSDKInstallerType()
                ?? InstallerType.Unknown;

            PackageUri = new(Installer.InstallerUrl);

            Status = PackageStatus.DownloadReady;
        }

        private string GetInstallerErrorMessage(long code)
        {
            var errorCodes = Installer.ExpectedReturnCodes ?? Manifest.ExpectedReturnCodes;
            var expectedReturnCode = errorCodes?.Find(erc => erc.InstallerReturnCode == code);
            if (expectedReturnCode == null)
                return $"Installer exited with code {code}.";

            return expectedReturnCode.ReturnResponse switch
            {
                ExpectedReturnCodeReturnResponse.PackageInUse => "The package is currently in use.",
                ExpectedReturnCodeReturnResponse.InstallInProgress => "Another install in already in progress.",
                ExpectedReturnCodeReturnResponse.FileInUse => "Attempted to access a file being used by another program.",
                ExpectedReturnCodeReturnResponse.MissingDependency => "A required dependency was missing.",
                ExpectedReturnCodeReturnResponse.DiskFull => "The target drive is out of storage space.",
                ExpectedReturnCodeReturnResponse.InsufficientMemory => "Your system is out of memory.",
                ExpectedReturnCodeReturnResponse.NoNetwork => "An internet connection is required.",
                ExpectedReturnCodeReturnResponse.ContactSupport => SupportUrl == null ? "Please contact support." : $"Please contact support at {SupportUrl.Uri}.",
                ExpectedReturnCodeReturnResponse.RebootRequiredToFinish or
                ExpectedReturnCodeReturnResponse.RebootRequiredForInstall => "Your computer needs to restart to complete installation.",
                ExpectedReturnCodeReturnResponse.RebootInitiated => "Your computer is restarting.",
                ExpectedReturnCodeReturnResponse.CancelledByUser => "Installation was cancelled.",
                ExpectedReturnCodeReturnResponse.AlreadyInstalled => $"{Title} is already installed.",
                ExpectedReturnCodeReturnResponse.Downgrade => $"A newer version of {Title} is already installed.",
                ExpectedReturnCodeReturnResponse.BlockedByPolicy => $"Installation of {Title} was blocked by an adminitrator policy.",

                _ => $"An unknown error occurred: {expectedReturnCode.ReturnResponse}."
            };
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

        private InstallerManifest _Manifest;
        public InstallerManifest Manifest
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

        private Link _SupportUrl;
        public Link SupportUrl
        {
            get => _SupportUrl;
            set => SetProperty(ref _SupportUrl, value);
        }
    }
}
