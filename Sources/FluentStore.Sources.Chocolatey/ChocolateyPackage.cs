using Chocolatey;
using Chocolatey.Models;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Attributes;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.Sources.Chocolatey
{
    public class ChocolateyPackage : PackageBase<Package>
    {
        private readonly IChocoPackageService _pkgMan;

        public ChocolateyPackage(PackageHandlerBase packageHandler, IChocoPackageService pkgMan, Package pack = null) : base(packageHandler)
        {
            _pkgMan = pkgMan;

            if (pack != null)
                Update(pack);
        }

        public void Update(Package pack)
        {
            Guard.IsNotNull(pack, nameof(pack));
            Model = pack;

            // Set URN
            string urn = $"urn:{ChocolateyHandler.NAMESPACE_CHOCO}:{Model.Id}";
            if (Model.Version != null)
                urn += ":" + Model.Version.ToString();
            Urn = Urn.Parse(urn);

            // Set base properties
            Title = pack.Title;
            PackageId = pack.Id;
            DeveloperName = pack.AuthorName;
            ReleaseDate = pack.Created;
            Description = pack.Description;
            Version = pack.Version.ToString();
            Website = Link.Create(pack.ProjectUrl, "Project website");

            // Set Choco package properties
            DownloadCountDisplay = pack.DownloadCount.ToMetric();
            Tags = pack.Tags.ToList();

            Links.Clear();

            if (!string.IsNullOrEmpty(pack.DocsUrl))
                Links.Add(Link.Create(pack.DocsUrl, "Docs"));

            if (!string.IsNullOrEmpty(pack.BugTrackerUrl))
                Links.Add(Link.Create(pack.BugTrackerUrl, "Bug tracker"));

            if (!string.IsNullOrEmpty(pack.PackageSourceUrl))
                Links.Add(Link.Create(pack.PackageSourceUrl, "Source"));

            if (!string.IsNullOrEmpty(pack.MailingListUrl))
                Links.Add(Link.Create(pack.MailingListUrl, "Mailing list"));

        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            // Find the package URI
            await PopulatePackageUri();
            if (PackageUri is null)
                return null;

            // Download package
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);
            if (!IsDownloaded)
                return null;

            // Set the proper file name
            DownloadItem = ((FileInfo)DownloadItem).CopyRename($"{Model.Id}_{Model.Version}.nupkg");

            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(this));
            return DownloadItem;
        }

        private async Task PopulatePackageUri()
        {
            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            try
            {
                PackageUri = new(Model.DownloadUrl);

                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));
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

        public override async Task<ImageBase> CacheHeroImage() => null;

        public override async Task<List<ImageBase>> CacheScreenshots() => new List<ImageBase>();

        public override async Task<bool> InstallAsync()
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(this));

                Progress<PackageProgress> progress = new(p =>
                {
                    WeakReferenceMessenger.Default.Send(
                        new PackageDownloadProgressMessage(this, p.Percentage, 100));
                });

                bool isSuccess = await _pkgMan.InstallAsync(PackageId, progress: progress);
                if (!isSuccess)
                    throw new Exception();

                WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(this));
                return true;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageInstallFailed));
                return false;
            }
        }

        public override Task<bool> CanDownloadAsync() => Task.FromResult(Model?.DownloadUrl is not null);

        public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

        public override Task LaunchAsync() => Task.CompletedTask;

        private string _PackageId;
        [DisplayAdditionalInformation("Package ID", "\uE625")]
        public string PackageId
        {
            get => _PackageId;
            set => SetProperty(ref _PackageId, value);
        }

        private string _DownloadCountDisplay;
        [DisplayAdditionalInformation("Download count", "\uE896")]
        public string DownloadCountDisplay
        {
            get => _DownloadCountDisplay;
            set => SetProperty(ref _DownloadCountDisplay, value);
        }

        private List<Link> _Links = [];
        [DisplayAdditionalInformation("Links", "\uE71B")]
        public List<Link> Links
        {
            get => _Links;
            set => SetProperty(ref _Links, value);
        }

        private List<string> _Tags = [];
        [DisplayAdditionalInformation(Icon = "\uE8EC")]
        public List<string> Tags
        {
            get => _Tags;
            set => SetProperty(ref _Tags, value);
        }
    }
}
