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
using System.Text;

namespace FluentStore.SDK
{
    public abstract partial class PackageBase : ObservableObject, IEquatable<PackageBase>, IHasAccessibleDescription
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
        /// and <see cref="CanInstallAsync"/> requires the package to be downloaded first.
        /// </summary>
        public virtual bool RequiresDownloadForCompatCheck => false;

        /// <summary>
        /// Gets a message describing why this package cannot be installed on this system.
        /// </summary>
        /// <returns><c>null</c> if it can be installed, a reason if it can't.</returns>
        public virtual Task<string> GetCannotBeInstalledReason() => Task.FromResult<string>(null);

        /// <summary>
        /// Determines if this package can be installed on this system.
        /// </summary>
        public async Task<bool> CanInstallAsync() => await GetCannotBeInstalledReason() == null;

        public abstract Task<bool> InstallAsync();

        public abstract Task<bool> CanDownloadAsync();

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
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
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

        [ObservableProperty]
        private PackageStatus _status = PackageStatus.Unknown;

        [ObservableProperty]
        private bool _isDownloaded;

        // TODO: Have some mechanism to determine whether updates are available
        [ObservableProperty]
        private bool _isInstalled;

        [ObservableProperty]
        private FileSystemInfo _downloadItem;

        [ObservableProperty]
        private InstallerType _type;

        [ObservableProperty]
        private Uri _packageUri;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _publisherId;

        [ObservableProperty]
        private string _developerName;

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

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _version;

        [ObservableProperty]
        private ReviewSummary _reviewSummary;
        public bool HasReviewSummary => ReviewSummary != null;

        [ObservableProperty]
        private double _price = -1;
        public bool HasPrice => Price > 0;

        [ObservableProperty]
        private string _displayPrice;
        public bool HasDisplayPrice => DisplayPrice != null;

        private string _shortTitle;
        public string ShortTitle
        {
            get => string.IsNullOrEmpty(_shortTitle) ? Title : _shortTitle;
            set => _shortTitle = value;
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

        [ObservableProperty]
        private List<ImageBase> _images = [];

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

        public virtual string ToAccessibleDescription()
        {
            StringBuilder sb = new(ShortTitle ?? Title);
            if (DeveloperName is not null)
                sb.AppendFormat(" by {0}", DeveloperName);
            return sb.ToString();
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
    }
}
