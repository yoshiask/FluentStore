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
using StoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Profile;
using System.IO;
using Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentStore.SDK.Packages
{
    public class MicrosoftStorePackage : PackageBase<ProductDetails>
    {
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
            Website = product.AppWebsiteUrl;
            StoreId = product.ProductId;

            // Set MS Store package properties
            Notes = product.Notes;
            Features = product.Features;
            Categories = product.Categories;
            PrivacyUrl = product.PrivacyUrl;
            Platforms = product.Platforms;
            if (product.SupportUris != null)
                foreach (SupportUri uri in product.SupportUris)
                    SupportUrls.Add(uri.Url);
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

            ReviewSummary.Reviews = new List<Models.Review>();
            foreach (Microsoft.Marketplace.Storefront.Contracts.V3.Review msReview in reviewList.Reviews)
            {
                Models.Review review = new()
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

        public void Update(PackageInstance packageInstance)
        {
            Guard.IsNotNull(packageInstance, nameof(packageInstance));

            Version = packageInstance.Version.ToString();
            PackageMoniker = packageInstance.PackageMoniker;
            PackageUri = packageInstance.PackageUri;

            PackageBase internalPackage = InternalPackage;
            CopyProperties(ref internalPackage);
            InternalPackage = internalPackage;
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
                    _Urn = Urn.Parse("urn:" + Handlers.MicrosoftStoreHandler.NAMESPACE_MSSTORE + ":" + StoreId);
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
            DownloadItem = await InternalPackage.DownloadPackageAsync(folder);
            Status = InternalPackage.Status;

            // Check for success
            if (Status.IsLessThan(PackageStatus.Downloaded))
                return null;

            // Set the proper file type and extension
            FileInfo downloadFile = (FileInfo)DownloadItem;
            string filename;
            if (!IsWinGet)
                filename = PackageMoniker + ((ModernPackage<ProductDetails>)InternalPackage).GetInstallerType();
            else
                filename = Path.GetFileName(PackageUri.ToString());
            if (filename != string.Empty)
                downloadFile.Rename(filename);

            WeakReferenceMessenger.Default.Send(new PackageDownloadCompletedMessage(this, downloadFile));
            DownloadItem = downloadFile;
            return DownloadItem;
        }

        private async Task<bool> PopulatePackageUri()
        {
            PlatWindows currentPlat = PlatWindowsStringConverter.Parse(AnalyticsInfo.VersionInfo.DeviceFamily);
            var culture = System.Globalization.CultureInfo.CurrentUICulture;

            if (!IsWinGet)
            {
                var dcathandler = new StoreLib.Services.DisplayCatalogHandler(DCatEndpoint.Production, new StoreLib.Services.Locale(culture, true));
                await dcathandler.QueryDCATAsync(StoreId);
                IEnumerable<PackageInstance> packs = await dcathandler.GetMainPackagesForProductAsync();

                if (currentPlat != PlatWindows.Xbox)
                    packs = packs.Where(p => p.Version.Revision != 70);
                List<PackageInstance> installables = packs.OrderByDescending(p => p.Version).ToList();
                if (installables.Count < 1)
                    return false;

                // TODO: Add addtional checks that might take longer that the user can enable 
                // if they are having issues

                Update(installables.First());
            }
            else
            {
                var StoreEdgeFDApi = Ioc.Default.GetRequiredService<Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.StoreEdgeFDApi>();
                Update((await StoreEdgeFDApi.GetPackageManifest(StoreId)).Data.Versions[0]);
            }

            Status = PackageStatus.DownloadReady;
            return true;
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
            return icons.OrderByDescending(i => i.Width).First();
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

            return img ?? (await GetScreenshots()).FirstOrDefault() ?? null;
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
                await InternalPackage.InstallAsync();
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

        private string _PrivacyUrl;
        public string PrivacyUrl
        {
            get => _PrivacyUrl;
            set
            {
                SetProperty(ref _PrivacyUrl, value);
                try
                {
                    SetProperty(ref _PrivacyUri, new Uri(value));
                }
                catch { }
            }
        }

        private Uri _PrivacyUri;
        [DisplayAdditionalInformation("Privacy url", "\uE71B")]
        public Uri PrivacyUri
        {
            get => _PrivacyUri;
            set
            {
                SetProperty(ref _PrivacyUri, value);
                SetProperty(ref _PrivacyUrl, value.ToString());
            }
        }

        private List<string> _Platforms = new();
        public List<string> Platforms
        {
            get => _Platforms;
            set => SetProperty(ref _Platforms, value);
        }

        private List<string> _SupportUrls = new();
        public List<string> SupportUrls
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

        public bool IsWinGet => StoreId.StartsWith("XP");

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
    }
}
