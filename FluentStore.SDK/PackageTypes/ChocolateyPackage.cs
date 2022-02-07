using Chocolatey;
using Chocolatey.Models;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    public class ChocolateyPackage : PackageBase<Package>
    {
        public ChocolateyPackage(Package pack = null)
        {
            if (pack != null)
                Update(pack);
        }

        public void Update(Package pack)
        {
            Guard.IsNotNull(pack, nameof(pack));
            Model = pack;

            // Set base properties
            Title = pack.Title;
            PackageId = pack.Id;
            DeveloperName = pack.AuthorName;
            ReleaseDate = pack.Created;
            Description = pack.Description;
            Version = pack.Version.ToString();
            Website = Link.Create(pack.ProjectUrl, "Project website");

            // Set Choco package properties
            Links = new[]
            {
                Link.Create(pack.DocsUrl, ShortTitle + " docs"),
                Link.Create(pack.BugTrackerUrl, ShortTitle + " bug tracker"),
                Link.Create(pack.PackageSourceUrl, ShortTitle + " source"),
                Link.Create(pack.MailingListUrl, ShortTitle + " mailing list"),
            };
        }

        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                {
                    string urn = $"urn:{Handlers.ChocolateyHandler.NAMESPACE_CHOCO}:{Model.Id}";
                    if (Model.Version != null)
                        urn += ":" + Model.Version.ToString();
                    _Urn = Urn.Parse(urn);
                }
                return _Urn;
            }
            set => _Urn = value;
        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            // Find the package URI
            await PopulatePackageUri();
            if (!Status.IsAtLeast(PackageStatus.DownloadReady))
                return null;

            // Download package
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);
            if (!Status.IsAtLeast(PackageStatus.Downloaded))
                return null;

            // Set the proper file name
            DownloadItem = ((FileInfo)DownloadItem).CopyRename($"{Model.Id}.{Model.Version}.nupkg");

            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(this));
            Status = PackageStatus.Downloaded;
            return DownloadItem;
        }

        private async Task PopulatePackageUri()
        {
            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            try
            {
                if (PackageUri == null)
                    PackageUri = new(Model.DownloadUrl);

                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));
                Status = PackageStatus.DownloadReady;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageFetchFailed));
            }
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            ImageBase icon = null;
            if (Model?.IconUrl != null)
            {
                icon = new FileImage
                {
                    Url = Model.IconUrl,
                    ImageType = ImageType.Logo
                };
            }

            return icon ?? TextImage.CreateFromName(Model.Title);
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

            try
            {
                // Extract nupkg and locate PowerShell install script
                var dir = StorageHelper.ExtractArchiveToDirectory((FileInfo)DownloadItem, true);
                FileInfo installer = new(Path.Combine(dir.FullName, "tools", "chocolateyInstall.ps1"));
                DownloadItem = installer;

                // Run install script
                // Cannot find a provider with the name '$ErrorActionPreference = 'Stop';
                using PowerShell ps = PowerShell.Create();
                var results = await ps.AddScript(File.ReadAllText(installer.FullName))
                                      .InvokeAsync();
                
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageInstallFailed));
                return false;
            }

            if (isSuccess)
            {
                WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(this));
                Status = PackageStatus.Installed;
            }
            return isSuccess;
        }

        public override Task<bool> CanLaunchAsync()
        {
            return Task.FromResult(false);
        }

        public override Task LaunchAsync()
        {
            return Task.CompletedTask;
        }

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

        private Link[] _Links;
        public Link[] Links
        {
            get => _Links;
            set => SetProperty(ref _Links, value);
        }
    }
}
