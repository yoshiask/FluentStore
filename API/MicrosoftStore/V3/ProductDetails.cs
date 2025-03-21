﻿using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ProductDetails
    {
        public List<string> Platforms { get; set; }
        public string PrivacyUrl { get; set; }
        public bool Accessible { get; set; }
        public bool IsDeviceCompanionApp { get; set; }
        public List<V2.SupportUri> SupportUris { get; set; }
        public List<string> Features { get; set; }
        public List<string> Notes { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public string PublisherCopyrightInformation { get; set; }
        public string AdditionalLicenseTerms { get; set; }
        public string AppWebsiteUrl { get; set; }
        public List<V2.ProductRating> ProductRatings { get; set; }
        public List<string> PermissionsRequired { get; set; }
        public List<string> PackageAndDeviceCapabilities { get; set; }
        public string Version { get; set; }
        public DateTimeOffset LastUpdateDateUtc { get; set; }
        public List<Sku> Skus { get; set; }
        public string CategoryId { get; set; }
        public string SubcategoryId { get; set; }
        public string NavItemId { get; set; }
        public string DeviceFamilyDisallowedReason { get; set; }
        public string BuiltFor { get; set; }
        public List<VideoItem> Trailers { get; set; }
        public string RevisionId { get; set; }
        public string PdpBackgroundColor { get; set; }
        public bool ContainsDownloadPackage { get; set; }
        public SystemRequirements SystemRequirements { get; set; }
        public List<string> KeyIds { get; set; }
        public List<PlatWindows> AllowedPlatforms { get; set; }
        public string InstallationTerms { get; set; }
        public bool XboxXpa { get; set; }
        public DetailsDisplayConfiguration DetailsDisplayConfiguration { get; set; }
        public List<WarningMessage> WarningMessages { get; set; }
        public bool IsMicrosoftProduct { get; set; }
        public bool HasParentBundles { get; set; }
        public bool HasAlternateEditions { get; set; }
        public ProductPartD ProductPartD { get; set; }
        public int VideoProductType { get; set; }
        public bool IsMsixvc { get; set; }
        public List<string> Categories { get; set; }
        public List<V2.ImageItem> Images { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string DeveloperName { get; set; }
        public string PublisherName { get; set; }
        public string PublisherId { get; set; }
        public bool IsUniversal { get; set; }
        public string Language { get; set; }
        public double Price { get; set; }
        public string DisplayPrice { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public bool HasFreeTrial { get; set; }
        public string ProductType { get; set; }
        public string ProductFamilyName { get; set; }
        public string MediaType { get; set; }
        public List<string> ContentIds { get; set; }
        public List<string> PackageFamilyNames { get; set; }
        public string SubcategoryName { get; set; }
        public List<AlternateId> AlternateIds { get; set; }
        public string CollectionItemType { get; set; }
        public int NumberOfSeasons { get; set; }
        public DateTimeOffset ReleaseDateUtc { get; set; }
        public long DurationInSeconds { get; set; }
        public bool IsCompatible { get; set; }
        public bool IsPurchaseEnabled { get; set; }
        public bool DeveloperOptOutOfSDCardInstall { get; set; }
        public bool HasAddOns { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public string VoiceTitle { get; set; }
        public List<object> CapabilitiesTable { get; set; }
        public List<object> Capabilities { get; set; }
        public bool HideFromCollections { get; set; }
        public bool IsDownloadable { get; set; }
        public bool HideFromDownloadsAndUpdates { get; set; }
        public bool GamingOptionsXboxLive { get; set; }
        public List<ActionOverride> ActionOverrides { get; set; }
        public string AvailableDevicesDisplayText { get; set; }
        public string AvailableDevicesNarratorText { get; set; }
        public long ApproximateSizeInBytes { get; set; }
        public long MaxInstallSizeInBytes { get; set; }
        public bool IsGamingAppOnly { get; set; }
        public V9.Installer Installer { get; set; }
        public string CatalogSource { get; set; }

        [JsonIgnore]
        public Uri PrivacyUri
            => Uri.TryCreate(PrivacyUrl, UriKind.Absolute, out var uri)
            ? uri : null;

        [JsonIgnore]
        public Uri AppWebsiteUri
            => Uri.TryCreate(AppWebsiteUrl, UriKind.Absolute, out var uri)
            ? uri : null;
    }

    public class SystemRequirements
    {
        public SpecificationCollection Minimum { get; set; }
        public SpecificationCollection Recommended { get; set; }
    }
}
