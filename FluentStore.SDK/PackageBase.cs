using FluentStore.SDK.Images;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using FluentStore.SDK.Attributes;
using OwlCore.AbstractUI.Models;

namespace FluentStore.SDK
{
    public abstract class PackageBase : ObservableObject, IEquatable<PackageBase>
    {
        protected static readonly List<AbstractButton> _emptyCommandList = new(0);

        public PackageBase(PackageHandlerBase packageHandler)
        {
            PackageHandler = packageHandler;
        }

        /// <summary>
        /// Copies the properties this <see cref="PackageBase"/> to the supplied instance.
        /// </summary>
        public void CopyProperties<TPackage>(ref TPackage other, bool copyStatus = false) where TPackage : PackageBase
        {
            other.Urn = Urn;
            other.Model = Model;
            other.DownloadItem = DownloadItem;
            other.Type = Type;
            other.PackageUri = PackageUri;
            other.Title = Title;
            other.PublisherId = PublisherId;
            other.DeveloperName = DeveloperName;
            other.ReleaseDate = ReleaseDate;
            other.Description = Description;
            other.Version = Version;
            other.ReviewSummary = ReviewSummary;
            other.Price = Price;
            other.DisplayPrice = DisplayPrice;
            other.ShortTitle = ShortTitle;
            other.Website = Website;
            other.PrivacyUri = PrivacyUri;
            other.Images = Images;

            other.AppIconCache ??= AppIconCache;
            other.HeroImageCache ??= HeroImageCache;

            if (copyStatus)
                other.Status = Status;
        }

        /// <summary>
        /// A Uniform Resource Name (URN) that represents this specific package.
        /// </summary>
        /// <remarks>
        /// <see cref="Urn.NamespaceIdentifier"/> is the name of the <see cref="PackageHandlerBase"/>
        /// that handles this package, <see cref="Urn.UnEscapedValue"/> is the handler-specific
        /// ID of this package.
        /// </remarks>
        public Urn Urn { get; set; }

        /// <summary>
        /// Gets the <see cref="PackageHandlerBase"/> that created this package.
        /// </summary>
        public PackageHandlerBase PackageHandler { get; }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether <see cref="GetCannotBeInstalledReason"/>
        /// and <see cref="CanBeInstalled"/> requires the package to be downloaded first.
        /// </summary>
        public virtual bool RequiresDownloadForCompatCheck => false;

        /// <summary>
        /// Gets a message describing why this package cannot be installed on this system.
        /// </summary>
        /// <returns><c>null</c> if it can be installed, a reason if it can't.</returns>
        public virtual Task<string> GetCannotBeInstalledReason() => null;

        /// <summary>
        /// Determines if this package can be installed on this system.
        /// </summary>
        public bool CanBeInstalled() => GetCannotBeInstalledReason() == null;

        public abstract Task<bool> InstallAsync();

        public abstract Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null);

        public abstract Task<bool> CanLaunchAsync();

        public abstract Task LaunchAsync();

        /// <summary>
        /// Gets additional install/download commands.
        /// </summary>
        /// <remarks>
        /// The official Fluent Store app shows these in the Install/Launch
        /// split button drop down menu, below the "Download installer"
        /// option.
        /// </remarks>
        public virtual List<AbstractButton> GetAdditionalCommands() => _emptyCommandList;

        public virtual bool Equals(PackageBase other) => this.Urn?.EscapedValue == other?.Urn?.EscapedValue
            && this.Status == other?.Status;

        public override bool Equals(object obj) => obj is PackageBase other ? this.Equals(other) : false;

        public static bool operator ==(PackageBase lhs, PackageBase rhs)
        {
            // Equals handles case of null on right side.
            return (lhs is null && rhs is null) || lhs.Equals(rhs);
        }

        public static bool operator !=(PackageBase lhs, PackageBase rhs)
            => (lhs is null ^ rhs is null) && !lhs.Equals(rhs);

        public override int GetHashCode() => Urn.GetHashCode();

        public override string ToString() => Title;

        /// <summary>
        /// The underlying model for this package class.
        /// </summary>
        private object _Model;
        public virtual object Model
        {
            get => _Model;
            set => SetProperty(ref _Model, value);
        }

        private PackageStatus _Status = PackageStatus.Unknown;
        public PackageStatus Status
        {
            get => _Status;
            set => SetProperty(ref _Status, value);
        }

        private FileSystemInfo _DownloadItem;
        public FileSystemInfo DownloadItem
        {
            get => _DownloadItem;
            set => SetProperty(ref _DownloadItem, value);
        }

        private InstallerType _Type;
        public InstallerType Type
        {
            get => _Type;
            set => SetProperty(ref _Type, value);
        }

        private Uri _PackageUri;
        public Uri PackageUri
        {
            get => _PackageUri;
            set => SetProperty(ref _PackageUri, value);
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _PublisherId;
        public string PublisherId
        {
            get => _PublisherId;
            set => SetProperty(ref _PublisherId, value);
        }

        private string _DeveloperName;
        public string DeveloperName
        {
            get => _DeveloperName;
            set => SetProperty(ref _DeveloperName, value);
        }

        /// <summary>
        /// The date this specific package was released.
        /// </summary>
        private DateTimeOffset _ReleaseDate;
        [DisplayAdditionalInformation("Release date", "\uE163")]
        public DateTimeOffset ReleaseDate
        {
            get => _ReleaseDate;
            set => SetProperty(ref _ReleaseDate, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        private string _Version;
        public string Version
        {
            get => _Version;
            set => SetProperty(ref _Version, value);
        }

        private ReviewSummary _ReviewSummary;
        public ReviewSummary ReviewSummary
        {
            get => _ReviewSummary;
            set => SetProperty(ref _ReviewSummary, value);
        }
        public bool HasReviewSummary => ReviewSummary != null;

        private double _Price = -1;
        public double Price
        {
            get => _Price;
            set => SetProperty(ref _Price, value);
        }
        public bool HasPrice => Price >= 0;

        private string _DisplayPrice;
        public string DisplayPrice
        {
            get => _DisplayPrice;
            set => SetProperty(ref _DisplayPrice, value);
        }
        public bool HasDisplayPrice => DisplayPrice != null;

        private string _ShortTitle;
        public string ShortTitle
        {
            get => string.IsNullOrEmpty(_ShortTitle) ? Title : _ShortTitle;
            set => _ShortTitle = value;
        }

        private Link _Website;
        [DisplayAdditionalInformation("Website", "\uE71B")]
        public Link Website
        {
            get => _Website;
            set => SetProperty(ref _Website, value);
        }
        public bool HasWebsite => Website != null;

        private Link _PrivacyUri;
        [DisplayAdditionalInformation("Privacy url", "\uE928")]
        public Link PrivacyUri
        {
            get => _PrivacyUri;
            set => SetProperty(ref _PrivacyUri, value);
        }

        private List<ImageBase> _Images = new();
        public List<ImageBase> Images
        {
            get => _Images;
            set => SetProperty(ref _Images, value);
        }

        public ImageBase AppIconCache;
        /// <summary>
        /// Populates the image cache for the app icon.
        /// </summary>
        public abstract Task<ImageBase> CacheAppIcon();

        /// <summary>
        /// Gets the app icon.
        /// </summary>
        /// <remarks>
        /// Uses the image cache if populated.
        /// </remarks>
        public async Task<ImageBase> GetAppIcon()
        {
            if (AppIconCache == null)
                AppIconCache = await CacheAppIcon();
            return AppIconCache;
        }

        public ImageBase HeroImageCache;
        /// <summary>
        /// Populates the image cache for the hero image.
        /// </summary>
        public abstract Task<ImageBase> CacheHeroImage();

        /// <summary>
        /// Gets the hero image.
        /// </summary>
        /// <remarks>
        /// Uses the image cache if populated.
        /// </remarks>
        public async Task<ImageBase> GetHeroImage()
        {
            if (HeroImageCache == null)
                HeroImageCache = await CacheHeroImage();
            return HeroImageCache;
        }

        public List<ImageBase> ScreenshotsCache;
        /// <summary>
        /// Populates the image cache for screenshots.
        /// </summary>
        public abstract Task<List<ImageBase>> CacheScreenshots();

        /// <summary>
        /// Gets the screenshots.
        /// </summary>
        /// <remarks>
        /// Uses the image cache if populated.
        /// </remarks>
        public async Task<List<ImageBase>> GetScreenshots()
        {
            if (ScreenshotsCache == null)
                ScreenshotsCache = await CacheScreenshots();
            return ScreenshotsCache;
        }
    }

    public abstract class PackageBase<TModel> : PackageBase
    {
        public PackageBase(PackageHandlerBase packageHandler) : base(packageHandler)
        {

        }

        private TModel _Model;
        public new TModel Model
        {
            get => _Model;
            set => SetProperty(ref _Model, value);
        }
    }

    public enum PackageStatus
    {
        Unknown,

        None,

        /// <summary>
        /// Only the basic details have been retrieved from the source.
        /// Suitable for search suggestions and results.
        /// </summary>
        BasicDetails,

        /// <summary>
        /// All details immediately available for the package have been retrieved from the source.
        /// Suitable for PackageView.
        /// </summary>
        Details,

        /// <summary>
        /// The package is ready to be downloaded.
        /// </summary>
        DownloadReady,

        /// <summary>
        /// The package has been successfully downloaded, and <see cref="PackageBase.DownloadItem"/> has been populated.
        /// </summary>
        Downloaded,

        /// <summary>
        /// The package has been successfully installed.
        /// </summary>
        Installed,
    }
}
