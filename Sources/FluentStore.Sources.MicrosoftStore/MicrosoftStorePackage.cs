using FluentStore.SDK.Attributes;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Microsoft.Marketplace.Storefront.Contracts.V2;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Profile;
using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest;
using StoreDownloader;
using FluentStore.SDK;
using FluentStore.SDK.Packages;
using FluentStore.Sources.WinGet;

namespace FluentStore.Sources.MicrosoftStore
{
    public class MicrosoftStorePackage : PackageBase<ProductDetails>
    {
        private const string ACCESSIBILITY_NOTICE_TEXT =
            "The app developer believes this " +
            "app meets accessibility requiements, " +
            "making it easier for everyone to use.";

        public MicrosoftStorePackage(CardModel card = null, ProductSummary summary = null, ProductDetails product = null)
        {
            if (card != null)
                Update(card);
            if (summary != null)
                Update(summary);
            if (product != null)
                Update(product);
        }

        public void Update(CardModel card)
        {
            Guard.IsNotNull(card, nameof(card));
            //Model = product;

            // Set base properties
            Title = card.Title;
            Description = card.Description ?? card.LongDescription;
            if (card.RatingsCount != null)
            {
                ReviewSummary = new ReviewSummary
                {
                    AverageRating = card.AverageRating,
                    // TODO: Parse into int. Examples:
                    // 2K -> 2000; 1.2M -> 1200000
                    //ReviewCount = card.RatingsCount
                };
            }
            Price = card.Price;
            DisplayPrice = card.DisplayPrice;
            StoreId = card.ProductId;

            // Set MS Store package properties
            Categories = card.Categories;
            Images.Clear();
            if (card.Images != null)
                foreach (ImageItem img in card.Images)
                    Images.Add(new MicrosoftStoreImage(img));

            PackageBase internalPackage = InternalPackage;
            if (!IsWinGet)
            {
                var package = (ModernPackage<ProductDetails>)internalPackage;
                package.PackageFamilyName = card.PackageFamilyNames?[0];
                internalPackage = package;
            }
            Type = internalPackage.Type;
            CopyProperties(ref internalPackage);
            InternalPackage = internalPackage;
        }

        public void Update(ProductSummary summary)
        {
            Guard.IsNotNull(summary, nameof(summary));

            // Set base properties
            Title = summary.Title;
            if (summary.RatingCount > 0)
            {
                ReviewSummary = new ReviewSummary
                {
                    AverageRating = summary.AverageRating,
                    ReviewCount = summary.RatingCount
                };
            }
            Price = summary.Price;
            StoreId = summary.ProductId;

            // Set MS Store package properties
            Images.Clear();
            if (summary.Images != null)
                foreach (ImageItem img in summary.Images)
                    Images.Add(new MicrosoftStoreImage(img));
        }

        public void Update(ProductDetails product)
        {
            Guard.IsNotNull(product, nameof(product));
            Model = product;

            // Set base properties
            Title = product.Title;
            PublisherId = product.PublisherId;
            DeveloperName = product.PublisherName;  // TODO: This should be the developer name, but the MS Store has an empty string
            ReleaseDate = product.LastUpdateDateUtc;
            Description = product.Description;
            Version = product.Version;
            if (product.RatingCount > 0)
            {
                ReviewSummary = new ReviewSummary
                {
                    AverageRating = product.AverageRating,
                    ReviewCount = product.RatingCount
                };
            }
            Price = product.Price;
            DisplayPrice = product.DisplayPrice;
            ShortTitle = product.ShortTitle;
            Website = Link.Create(product.AppWebsiteUrl, ShortTitle + " website");
            StoreId = product.ProductId;

            // Set MS Store package properties
            Notes = product.Notes;
            Features = product.Features;
            Categories = product.Categories;
            PrivacyUri = Link.Create(product.PrivacyUri, ShortTitle + " privacy policy");
            Platforms = product.Platforms;
            if (product.SupportUris != null)
                foreach (SupportUri uri in product.SupportUris.Where(u => u.Uri != null))
                    SupportUrls.Add(new(uri.Uri, ShortTitle + " support"));
            Ratings = product.ProductRatings;
            PermissionsRequested = product.PermissionsRequested;
            PackageAndDeviceCapabilities = product.PackageAndDeviceCapabilities;
            AllowedPlatforms = product.AllowedPlatforms;
            WarningMessages = product.WarningMessages;
            Images.Clear();
            if (product.Images != null)
                foreach (ImageItem img in product.Images)
                    Images.Add(new MicrosoftStoreImage(img));

            PackageBase internalPackage = InternalPackage;
            if (!IsWinGet)
            {
                var package = (ModernPackage<ProductDetails>)internalPackage;
                package.PackageFamilyName = product.PackageFamilyNames?[0];
                package.PublisherDisplayName = product.PublisherName;
                internalPackage = package;
            }
            Type = internalPackage.Type;
            CopyProperties(ref internalPackage);
            InternalPackage = internalPackage;
        }

        public void Update(RatingSummary ratingSummary)
        {
            Guard.IsNotNull(ratingSummary, nameof(ratingSummary));

            ReviewSummary = new ReviewSummary
            {
                AverageRating = ratingSummary.AverageRating,
                ReviewCount = ratingSummary.ReviewCount,
                Star1Count = ratingSummary.Star1Count,
                Star2Count = ratingSummary.Star2Count,
                Star3Count = ratingSummary.Star3Count,
                Star4Count = ratingSummary.Star4Count,
                Star5Count = ratingSummary.Star5Count,
                Star1ReviewCount = ratingSummary.Star1ReviewCount,
                Star2ReviewCount = ratingSummary.Star2ReviewCount,
                Star3ReviewCount = ratingSummary.Star3ReviewCount,
                Star4ReviewCount = ratingSummary.Star4ReviewCount,
                Star5ReviewCount = ratingSummary.Star5ReviewCount
            };
        }

        public void Update(ReviewList reviewList)
        {
            Guard.IsNotNull(reviewList, nameof(reviewList));
            Guard.IsNotNull(ReviewSummary, nameof(ReviewSummary));

            ReviewSummary.Reviews = new List<SDK.Models.Review>();
            foreach (Microsoft.Marketplace.Storefront.Contracts.V3.Review msReview in reviewList.Reviews)
            {
                SDK.Models.Review review = new()
                {
                    Title = msReview.Title,
                    ReviewId = msReview.ReviewId.ToString(),
                    IsRevised = msReview.IsRevised,
                    Rating = (int)msReview.Rating,
                    ReviewerName = msReview.ReviewerName,
                    ReviewText = msReview.ReviewText,
                    Locale = msReview.Locale,
                    Market = msReview.Market,
                    HelpfulNegative = msReview.HelpfulNegative,
                    HelpfulPositive = msReview.HelpfulPositive,
                    SubmittedDateTimeUtc = msReview.SubmittedDateTimeUtc,
                    UpdatedSinceResponse = msReview.UpdatedSinceResponse,
                };
                ReviewSummary.Reviews.Add(review);
            }
        }

        public void Update(PackageManifestVersion manifest)
        {
            Guard.IsNotNull(manifest, nameof(manifest));
            Manifest = manifest;
            Version = manifest.PackageVersion;

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var installer = manifest.Installers.FirstOrDefault(i => i.InstallerLocale == culture.TwoLetterISOLanguageName && i.Markets.HasMarket(culture));
            PackageUri = installer.InstallerUri;
            Type = installer.InstallerType.ToSDKInstallerType();

            PackageBase internalPackage = InternalPackage;
            if (IsWinGet)
                ((WinGetPackage)InternalPackage).Update(manifest.ToWinGetRunManifest(installer));
            CopyProperties(ref internalPackage);
            InternalPackage = internalPackage;
        }

        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse("urn:" + MicrosoftStoreHandler.NAMESPACE_MSSTORE + ":" + StoreId);
                return _Urn;
            }
            set => _Urn = value;
        }

        public override bool RequiresDownloadForCompatCheck => false;
        public override async Task<string> GetCannotBeInstalledReason()
        {
            // Check Windows platform
            PlatWindows? currentPlat = PlatWindowsStringConverter.Parse(AnalyticsInfo.VersionInfo.DeviceFamily);
            if (!currentPlat.HasValue)
            {
                return "Cannot identify the current Windows platform.";
            }
            else if (AllowedPlatforms != null && !AllowedPlatforms.Contains(currentPlat.Value))
            {
                return Title + " does not support " + currentPlat.ToString();
            }

            // TODO: Check architecture, etc.

            return null;
        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            FileInfo downloadFile = (FileInfo)DownloadItem;

            if (IsWinGet)
            {
                // Find the package URI
                await PopulatePackageUri();
                if (Status.IsLessThan(PackageStatus.DownloadReady))
                    return null;

                // Download package
                downloadFile = (FileInfo)await InternalPackage.DownloadAsync(folder);
                Status = InternalPackage.Status;

                // Set the proper file type and extension
                string filename = Path.GetFileName(PackageUri.ToString());
                downloadFile.MoveRename(filename);
            }
            else
            {
                try
                {
                    // Get system and package info
                    string[] categoryIds = new[] { Model.Skus[0].FulfillmentData.WuCategoryId };
                    var sysInfo = Handlers.MicrosoftStore.Win32Helper.GetSystemInfo();
                    folder ??= StorageHelper.GetTempDirectory();

                    // Get update data
                    WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
                    var updates = await WindowsUpdateLib.FE3Handler.GetUpdates(categoryIds, sysInfo, "", WindowsUpdateLib.FileExchangeV3UpdateFilter.Application);
                    if (updates == null || !updates.Any())
                    {
                        WeakReferenceMessenger.Default.Send(new ErrorMessage(
                            WebException.Create(404, "No packages are available for " + ShortTitle), this, ErrorType.PackageFetchFailed));
                        return null;
                    }
                    WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));

                    // Set up progress handler
                    void DownloadProgress(DownloadLib.GeneralDownloadProgress progress)
                    {
                        var status = progress.DownloadedStatus[0];
                        WeakReferenceMessenger.Default.Send(
                            new PackageDownloadProgressMessage(this, status.DownloadedBytes, status.File.FileSize));
                    }

                    // Start download
                    WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(this));
                    string[] files = await MSStoreDownloader.DownloadPackageAsync(updates, folder, new DownloadProgress(DownloadProgress));
                    if (files == null || files.Length == 0)
                        throw new Exception("Failed to download pacakges using WindowsUpdateLib");
                    Status = InternalPackage.Status = PackageStatus.Downloaded;

                    InternalPackage.DownloadItem = downloadFile = new(Path.Combine(folder.FullName, files[0]));
                    await ((ModernPackage<ProductDetails>)InternalPackage).GetInstallerType();
                    Type = InternalPackage.Type;
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageDownloadFailed));
                    return null;
                }
            }

            if (Status.IsLessThan(PackageStatus.Downloaded))
                return null;

            DownloadItem = downloadFile;
            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(this));
            return DownloadItem;
        }

        private async Task PopulatePackageUri()
        {
            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentUICulture;

                if (!IsWinGet)
                {
                    throw new InvalidOperationException("PopulatePackageUri is only for WinGet-backed apps");
                }
                else
                {
                    var StoreEdgeFDApi = Ioc.Default.GetRequiredService<Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.StoreEdgeFDApi>();
                    Update((await StoreEdgeFDApi.GetPackageManifest(StoreId)).Data.Versions[0]);
                }

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
            // Prefer tiles, then logos, then posters.
            var icons = Images.FindAll(i => i.ImageType == SDK.Images.ImageType.Tile);
            if (icons.Count > 0)
                goto done;

            icons = Images.FindAll(i => i.ImageType == SDK.Images.ImageType.Logo);
            if (icons.Count > 0)
                goto done;

            icons = Images.FindAll(i => i.ImageType == SDK.Images.ImageType.Poster);
            if (icons.Count > 0)
                goto done;

            // If no app icon is specified, fall back to any image.
            icons = Images;

        done:
            var icon = icons.OrderByDescending(i => i.Width).First();
            if (InternalPackage != null)
                InternalPackage.AppIconCache = icon;
            return icon;
        }

        public override async Task<ImageBase> CacheHeroImage()
        {
            ImageBase img = null;
            int width = -1;
            foreach (ImageBase image in Images.FindAll(i => i.ImageType == SDK.Images.ImageType.Hero))
            {
                if (image.Width > width)
                {
                    img = image;
                    width = image.Width;
                }
            }

            img ??= (await GetScreenshots()).FirstOrDefault();
            if (InternalPackage != null)
                InternalPackage.HeroImageCache = img;
            return img;
        }

        public override async Task<List<ImageBase>> CacheScreenshots()
        {
            string deviceFam = AnalyticsInfo.VersionInfo.DeviceFamily["Windows.".Length..];
            var screenshots = Images.Cast<MicrosoftStoreImage>().Where(i => i.ImageType == SDK.Images.ImageType.Screenshot
                && !string.IsNullOrEmpty(i.ImagePositionInfo) && i.ImagePositionInfo.StartsWith(deviceFam, StringComparison.InvariantCultureIgnoreCase));

            List<ImageBase> sorted = new(screenshots.Count());
            foreach (MicrosoftStoreImage screenshot in screenshots)
            {
                // length + 1, to skip device family name and the "/"
                string posStr = screenshot.ImagePositionInfo[(deviceFam.Length + 1)..];
                int pos = int.Parse(posStr);
                if (pos >= sorted.Count)
                    sorted.Add(screenshot);
                else
                    sorted.Insert(pos, screenshot);
            }

            if (InternalPackage != null)
                InternalPackage.ScreenshotsCache = sorted;
            return sorted;
        }

        public override async Task<bool> InstallAsync()
        {
            if (InternalPackage != null)
                return await InternalPackage.InstallAsync();
            return false;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            if (InternalPackage != null)
                return await InternalPackage.CanLaunchAsync();
            return false;
        }

        public override async Task LaunchAsync()
        {
            if (InternalPackage != null)
                await InternalPackage.LaunchAsync();
        }

        private List<string> _Notes = new();
        [Display(Title = "What's new in this version", Rank = 3)]
        public List<string> Notes
        {
            get => _Notes;
            set => SetProperty(ref _Notes, value);
        }

        private List<string> _Features = new();
        [Display(Rank = 2)]
        public List<string> Features
        {
            get => _Features;
            set => SetProperty(ref _Features, value);
        }

        private List<string> _Categories = new();
        [DisplayAdditionalInformation(Icon = "\uE7C1")]
        public List<string> Categories
        {
            get => _Categories;
            set => SetProperty(ref _Categories, value);
        }

        private List<string> _Platforms = new();
        public List<string> Platforms
        {
            get => _Platforms;
            set => SetProperty(ref _Platforms, value);
        }

        private List<Link> _SupportUrls = new();
        public List<Link> SupportUrls
        {
            get => _SupportUrls;
            set => SetProperty(ref _SupportUrls, value);
        }

        private List<ProductRating> _Ratings = new();
        public List<ProductRating> Ratings
        {
            get => _Ratings;
            set => SetProperty(ref _Ratings, value);
        }

        private List<string> _PermissionsRequested = new();
        [DisplayAdditionalInformation("Permissions", "\uE8D7")]
        public List<string> PermissionsRequested
        {
            get => _PermissionsRequested;
            set => SetProperty(ref _PermissionsRequested, value);
        }

        private List<string> _PackageAndDeviceCapabilities = new();
        public List<string> PackageAndDeviceCapabilities
        {
            get => _PackageAndDeviceCapabilities;
            set => SetProperty(ref _PackageAndDeviceCapabilities, value);
        }

        private List<PlatWindows> _AllowedPlatforms = new();
        public List<PlatWindows> AllowedPlatforms
        {
            get => _AllowedPlatforms;
            set => SetProperty(ref _AllowedPlatforms, value);
        }

        private List<WarningMessage> _WarningMessages = new();
        [DisplayAdditionalInformation("Warnings", "\uE7BA")]
        public List<WarningMessage> WarningMessages
        {
            get => _WarningMessages;
            set => SetProperty(ref _WarningMessages, value);
        }

        private string _PackageMoniker;
        public string PackageMoniker
        {
            get => _PackageMoniker;
            set => SetProperty(ref _PackageMoniker, value);
        }

        private string _StoreId;
        public string StoreId
        {
            get => _StoreId;
            set => SetProperty(ref _StoreId, value);
        }

        public bool IsWinGet => Model?.Installer?.Type == "WPM" || StoreId.StartsWith("XP");

        private PackageManifestVersion _Manifest;
        /// <summary>
        /// The manifest for this package.
        /// </summary>
        /// <remarks>
        /// Only valid if this package is backed by WinGet.
        /// </remarks>
        public PackageManifestVersion Manifest
        {
            get => _Manifest;
            set => SetProperty(ref _Manifest, value);
        }

        private PackageBase _InternalPackage;
        /// <summary>
        /// The actual package type. Currently, packaged apps are <see cref="ModernPackage{ProductDetails}"/>
        /// and WinGet-backed apps are <see cref="WinGetPackage"/>.
        /// </summary>
        public PackageBase InternalPackage
        {
            get
            {
                if (_InternalPackage == null)
                {
                    if (IsWinGet)
                        _InternalPackage = new WinGetPackage();
                    else
                        _InternalPackage = new ModernPackage<ProductDetails>();
                }
                return _InternalPackage;
            }
            set => SetProperty(ref _InternalPackage, value);
        }

        [DisplayAdditionalInformation("Accessibility", "\uE776")]
        public string AccessibilityNotice => (Model != null && Model.Accessible) ? ACCESSIBILITY_NOTICE_TEXT : null;

        [DisplayAdditionalInformation("Supported languages", "\uE8F2")]
        public List<string> SupportedLanguages => Model?.SupportedLanguages;
    }
}
