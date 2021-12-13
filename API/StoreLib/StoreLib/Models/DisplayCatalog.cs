using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace StoreLib.Models
{
    public partial class DisplayCatalogModel
    {
        [JsonProperty("Product")]
        public Product Product { get; set; }
        [JsonProperty("BigIds")]
        public List<string> BigIds { get; set; }
        [JsonProperty("HasMorePages")]
        public bool HasMorePages { get; set; }
        [JsonProperty("Products")]
        public List<Product> Products { get; set; }
        [JsonProperty("TotalResultCount")]
        public int TotalResultCount { get; set; }
    }

    public partial class Product
    {
        [JsonProperty("LastModifiedDate")]
        public string LastModifiedDate { get; set; }

        [JsonProperty("LocalizedProperties")]
        public List<ProductLocalizedProperty> LocalizedProperties { get; set; }

        [JsonProperty("MarketProperties")]
        public List<ProductMarketProperty> MarketProperties { get; set; }

        [JsonProperty("ProductASchema")]
        public string ProductASchema { get; set; }

        [JsonProperty("ProductBSchema")]
        public string ProductBSchema { get; set; }

        [JsonProperty("Properties")]
        public ProductProperties Properties { get; set; }

        [JsonProperty("AlternateIds")]
        public List<AlternateId> AlternateIds { get; set; }

        [JsonProperty("DomainDataVersion")]
        public object DomainDataVersion { get; set; }

        [JsonProperty("IngestionSource")]
        public string IngestionSource { get; set; }

        [JsonProperty("IsMicrosoftProduct")]
        public bool IsMicrosoftProduct { get; set; }

        [JsonProperty("PreferredSkuId")]
        public string PreferredSkuId { get; set; }

        [JsonProperty("ProductType")]
        public string ProductType { get; set; }

        [JsonProperty("ValidationData")]
        public ValidationData ValidationData { get; set; }

        [JsonProperty("SandboxId")]
        public string SandboxID { get; set; }

        [JsonProperty("IsSandboxedProduct")]
        public bool IsSandboxedProduct { get; set; }

        [JsonProperty("MerchandizingTags")]
        public List<object> MerchandizingTags { get; set; }

        [JsonProperty("PartD")]
        public string PartD { get; set; }

        [JsonProperty("ProductFamily")]
        public string ProductFamily { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("ProductKind")]
        public string ProductKind { get; set; }

        [JsonProperty("DisplaySkuAvailabilities")]
        public List<DisplaySkuAvailability> DisplaySkuAvailabilities { get; set; }
    }

    public partial class ValidationData
    {
        [JsonProperty("PassedValidation")]
        public bool PassedValidation { get; set; }

        [JsonProperty("RevisionId")]
        public string RevisionId { get; set; }

        [JsonProperty("ValidationResultUri")]
        public string ValidationResultUri { get; set; }
    }

    public partial class ProductProperties
    {
        [JsonProperty("Attributes")]
        public List<object> Attributes { get; set; }

        [JsonProperty("CanInstallToSDCard")]
        public bool CanInstallToSdCard { get; set; }

        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("SubCategory")]
        public string SubCategory { get; set; }

        [JsonProperty("Categories")]
        public object Categories { get; set; }

        [JsonProperty("Extensions")]
        public object Extensions { get; set; }

        [JsonProperty("IsAccessible")]
        public bool IsAccessible { get; set; }

        [JsonProperty("IsLineOfBusinessApp")]
        public bool IsLineOfBusinessApp { get; set; }

        [JsonProperty("IsPublishedToLegacyWindowsPhoneStore")]
        public bool IsPublishedToLegacyWindowsPhoneStore { get; set; }

        [JsonProperty("IsPublishedToLegacyWindowsStore")]
        public bool IsPublishedToLegacyWindowsStore { get; set; }

        [JsonProperty("IsSettingsApp")]
        public bool IsSettingsApp { get; set; }

        [JsonProperty("PackageFamilyName")]
        public string PackageFamilyName { get; set; }

        [JsonProperty("PackageIdentityName")]
        public string PackageIdentityName { get; set; }

        [JsonProperty("PublisherCertificateName")]
        public string PublisherCertificateName { get; set; }

        [JsonProperty("PublisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("XboxLiveTier")]
        public object XboxLiveTier { get; set; }

        [JsonProperty("XboxXPA")]
        public object XboxXpa { get; set; }

        [JsonProperty("OwnershipType")]
        public object OwnershipType { get; set; }

        [JsonProperty("PdpBackgroundColor")]
        public string PdpBackgroundColor { get; set; }

        [JsonProperty("HasAddOns")]
        public bool? HasAddOns { get; set; }

        [JsonProperty("RevisionId")]
        public string RevisionId { get; set; }

        [JsonProperty("ProductGroupId")]
        public string ProductGroupId { get; set; }

        [JsonProperty("ProductGroupName")]
        public string ProductGroupName { get; set; }
    }

    public partial class ProductMarketProperty
    {
        [JsonProperty("OriginalReleaseDate")]
        public DateTime OriginalReleaseDate { get; set; }

        [JsonProperty("OriginalReleaseDateFriendlyName")]
        public string OriginalReleaseDateFriendlyName { get; set; }

        [JsonProperty("MinimumUserAge")]
        public long MinimumUserAge { get; set; }

        [JsonProperty("ContentRatings")]
        public List<ContentRating> ContentRatings { get; set; }

        [JsonProperty("RelatedProducts")]
        public List<object> RelatedProducts { get; set; }

        [JsonProperty("UsageData")]
        public List<UsageDatum> UsageData { get; set; }

        [JsonProperty("BundleConfig")]
        public object BundleConfig { get; set; }

        [JsonProperty("Markets")]
        public List<string> Markets { get; set; }
    }

    public partial class UsageDatum
    {
        [JsonProperty("AverageRating")]
        public double AverageRating { get; set; }

        [JsonProperty("AggregateTimeSpan")]
        public string AggregateTimeSpan { get; set; }

        [JsonProperty("RatingCount")]
        public long RatingCount { get; set; }

        [JsonProperty("PurchaseCount")]
        public long PurchaseCount { get; set; }

        [JsonProperty("TrialCount")]
        public long? TrialCount { get; set; }

        [JsonProperty("RentalCount")]
        public long RentalCount { get; set; }

        [JsonProperty("PlayCount")]
        public long PlayCount { get; set; }
    }

    public partial class ContentRating
    {
        [JsonProperty("RatingSystem")]
        public string RatingSystem { get; set; }

        [JsonProperty("RatingId")]
        public string RatingId { get; set; }

        [JsonProperty("RatingDescriptors")]
        public List<string> RatingDescriptors { get; set; }

        [JsonProperty("RatingDisclaimers")]
        public List<object> RatingDisclaimers { get; set; }

        [JsonProperty("InteractiveElements")]
        public List<string> InteractiveElements { get; set; }
    }

    public partial class ProductLocalizedProperty
    {
        [JsonProperty("DeveloperName")]
        public string DeveloperName { get; set; }

        [JsonProperty("DisplayPlatformProperties")]
        public object DisplayPlatformProperties { get; set; }

        [JsonProperty("PublisherName")]
        public string PublisherName { get; set; }

        [JsonProperty("PublisherWebsiteUri")]
        public string PublisherWebsiteUri { get; set; }

        [JsonProperty("SupportUri")]
        public string SupportUri { get; set; }

        [JsonProperty("EligibilityProperties")]
        public object EligibilityProperties { get; set; }

        [JsonProperty("Franchises")]
        public List<object> Franchises { get; set; }

        [JsonProperty("Images")]
        public List<Image> Images { get; set; }

        [JsonProperty("Videos")]
        public List<object> Videos { get; set; }

        [JsonProperty("ProductDescription")]
        public string ProductDescription { get; set; }

        [JsonProperty("ProductTitle")]
        public string ProductTitle { get; set; }

        [JsonProperty("ShortTitle")]
        public string ShortTitle { get; set; }

        [JsonProperty("SortTitle")]
        public string SortTitle { get; set; }

        [JsonProperty("ShortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("SearchTitles")]
        public List<SearchTitle> SearchTitles { get; set; }

        [JsonProperty("VoiceTitle")]
        public string VoiceTitle { get; set; }

        [JsonProperty("RenderGroupDetails")]
        public object RenderGroupDetails { get; set; }

        [JsonProperty("ProductDisplayRanks")]
        public List<object> ProductDisplayRanks { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Markets")]
        public List<string> Markets { get; set; }
    }

    public partial class SearchTitle
    {
        [JsonProperty("SearchTitleString")]
        public string SearchTitleString { get; set; }

        [JsonProperty("SearchTitleType")]
        public string SearchTitleType { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("BackgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty("Caption")]
        public string Caption { get; set; }

        [JsonProperty("FileSizeInBytes")]
        public long FileSizeInBytes { get; set; }

        [JsonProperty("ForegroundColor")]
        public string ForegroundColor { get; set; }

        [JsonProperty("Height")]
        public long Height { get; set; }

        [JsonProperty("ImagePositionInfo")]
        public string ImagePositionInfo { get; set; }

        [JsonProperty("ImagePurpose")]
        public string ImagePurpose { get; set; }

        [JsonProperty("UnscaledImageSHA256Hash")]
        public string UnscaledImageSha256Hash { get; set; }

        [JsonProperty("Uri")]
        public string Uri { get; set; }

        [JsonProperty("Width")]
        public long Width { get; set; }
    }

    public partial class DisplaySkuAvailability
    {
        [JsonProperty("Sku")]
        public Sku Sku { get; set; }

        [JsonProperty("Availabilities")]
        public List<Availability> Availabilities { get; set; }
    }

    public partial class Sku
    {
        [JsonProperty("LastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("LocalizedProperties")]
        public List<SkuLocalizedProperty> LocalizedProperties { get; set; }

        [JsonProperty("MarketProperties")]
        public List<SkuMarketProperty> MarketProperties { get; set; }

        [JsonProperty("Properties")]
        public SkuProperties Properties { get; set; }

        [JsonProperty("SkuASchema")]
        public string SkuASchema { get; set; }

        [JsonProperty("SkuBSchema")]
        public string SkuBSchema { get; set; }

        [JsonProperty("SkuId")]
        public string SkuId { get; set; }

        [JsonProperty("SkuType")]
        public string SkuType { get; set; }

        [JsonProperty("RecurrencePolicy")]
        public object RecurrencePolicy { get; set; }

        [JsonProperty("SubscriptionPolicyId")]
        public object SubscriptionPolicyId { get; set; }
    }

    public partial class SkuProperties
    {
        [JsonProperty("EarlyAdopterEnrollmentUrl")]
        public object EarlyAdopterEnrollmentUrl { get; set; }

        [JsonProperty("FulfillmentData")]
        public FulfillmentData FulfillmentData { get; set; }

        [JsonProperty("FulfillmentType")]
        public string FulfillmentType { get; set; }

        [JsonProperty("HasThirdPartyIAPs")]
        public bool HasThirdPartyIaPs { get; set; }

        [JsonProperty("LastUpdateDate")]
        public string LastUpdateDate { get; set; }

        [JsonProperty("HardwareProperties")]
        public HardwareProperties HardwareProperties { get; set; }

        [JsonProperty("HardwareRequirements")]
        public List<object> HardwareRequirements { get; set; }

        [JsonProperty("HardwareWarningList")]
        public List<object> HardwareWarningList { get; set; }

        [JsonProperty("InstallationTerms")]
        public string InstallationTerms { get; set; }

        [JsonProperty("Packages")]
        public List<Package> Packages { get; set; }

        [JsonProperty("VersionString")]
        public string VersionString { get; set; }

        [JsonProperty("VisibleToB2BServiceIds")]
        public List<object> VisibleToB2BServiceIds { get; set; }

        [JsonProperty("XboxXPA")]
        public bool XboxXpa { get; set; }

        [JsonProperty("BundledSkus")]
        public List<object> BundledSkus { get; set; }

        [JsonProperty("IsRepurchasable")]
        public bool? IsRepurchasable { get; set; }

        [JsonProperty("SkuDisplayRank")]
        public long SkuDisplayRank { get; set; }

        [JsonProperty("DisplayPhysicalStoreInventory")]
        public object DisplayPhysicalStoreInventory { get; set; }

        [JsonProperty("AdditionalIdentifiers")]
        public List<object> AdditionalIdentifiers { get; set; }

        [JsonProperty("IsTrial")]
        public bool IsTrial { get; set; }

        [JsonProperty("IsPreOrder")]
        public bool IsPreOrder { get; set; }

        [JsonProperty("IsBundle")]
        public bool IsBundle { get; set; }
    }

    public partial class Package
    {
        [JsonProperty("Applications")]
        public List<Application> Applications { get; set; }

        [JsonProperty("Architectures")]
        public List<string> Architectures { get; set; }

        [JsonProperty("Capabilities")]
        public List<string> Capabilities { get; set; }

        [JsonProperty("DeviceCapabilities")]
        public List<object> DeviceCapabilities { get; set; }

        [JsonProperty("ExperienceIds")]
        public List<object> ExperienceIds { get; set; }

        [JsonProperty("FrameworkDependencies")]
        public List<object> FrameworkDependencies { get; set; }

        [JsonProperty("HardwareDependencies")]
        public List<object> HardwareDependencies { get; set; }

        [JsonProperty("HardwareRequirements")]
        public List<object> HardwareRequirements { get; set; }

        [JsonProperty("Hash")]
        public string Hash { get; set; }

        [JsonProperty("HashAlgorithm")]
        public string HashAlgorithm { get; set; }

        [JsonProperty("IsStreamingApp")]
        public bool IsStreamingApp { get; set; }

        [JsonProperty("Languages")]
        public List<string> Languages { get; set; }

        [JsonProperty("MaxDownloadSizeInBytes")]
        public long MaxDownloadSizeInBytes { get; set; }

        [JsonProperty("MaxInstallSizeInBytes")]
        public string MaxInstallSizeInBytes { get; set; }

        [JsonProperty("PackageFormat")]
        public string PackageFormat { get; set; }

        [JsonProperty("PackageFamilyName")]
        public string PackageFamilyName { get; set; }

        [JsonProperty("MainPackageFamilyNameForDlc")]
        public object MainPackageFamilyNameForDlc { get; set; }

        [JsonProperty("PackageFullName")]
        public string PackageFullName { get; set; }

        [JsonProperty("PackageId")]
        public string PackageId { get; set; }

        [JsonProperty("ContentId")]
        public string ContentId { get; set; }

        [JsonProperty("KeyId")]
        public string KeyId { get; set; }

        [JsonProperty("PackageRank")]
        public long PackageRank { get; set; }

        [JsonProperty("PackageUri")]
        public string PackageUri { get; set; }

        [JsonProperty("PlatformDependencies")]
        public List<PlatformDependency> PlatformDependencies { get; set; }

        [JsonProperty("PlatformDependencyXmlBlob")]
        public string PlatformDependencyXmlBlob { get; set; }

        [JsonProperty("ResourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("PackageDownloadUris")]
        public List<PackageDownloadUris> PackageDownloadUris { get; set; }

        [JsonProperty("DriverDependencies")]
        public List<object> DriverDependencies { get; set; }

        [JsonProperty("FulfillmentData")]
        public FulfillmentData FulfillmentData { get; set; }


    }

    public partial class PackageDownloadUris
    {
        [JsonProperty("Uri")]
        public string Uri { get; set; }

        [JsonProperty("Rank")]
        public long Rank { get; set; }
    }

    public partial class PlatformDependency
    {
        [JsonProperty("MaxTested")]
        public long MaxTested { get; set; }

        [JsonProperty("MinVersion")]
        public long MinVersion { get; set; }

        [JsonProperty("PlatformName")]
        public string PlatformName { get; set; }
    }

    public partial class Application
    {
        [JsonProperty("ApplicationId")]
        public string ApplicationId { get; set; }

        [JsonProperty("DeclarationOrder")]
        public long DeclarationOrder { get; set; }

        [JsonProperty("Extensions")]
        public List<string> Extensions { get; set; }
    }

    public partial class HardwareProperties
    {
        [JsonProperty("MinimumHardware")]
        public List<object> MinimumHardware { get; set; }

        [JsonProperty("RecommendedHardware")]
        public List<string> RecommendedHardware { get; set; }

        [JsonProperty("MinimumProcessor")]
        public string MinimumProcessor { get; set; }

        [JsonProperty("RecommendedProcessor")]
        public string RecommendedProcessor { get; set; }

        [JsonProperty("MinimumGraphics")]
        public string MinimumGraphics { get; set; }

        [JsonProperty("RecommendedGraphics")]
        public string RecommendedGraphics { get; set; }
    }

    public partial class FulfillmentData
    {
        [JsonProperty("WuBundleId")]
        public string WuBundleId { get; set; }

        [JsonProperty("WuCategoryId")]
        public string WuCategoryId { get; set; }

        [JsonProperty("PackageFamilyName")]
        public string PackageFamilyName { get; set; }

        [JsonProperty("SkuId")]
        public string SkuId { get; set; }

        [JsonProperty("Content")]
        public object Content { get; set; }
    }

    public partial class SkuMarketProperty
    {
        [JsonProperty("FirstAvailableDate")]
        public string FirstAvailableDate { get; set; }

        [JsonProperty("SupportedLanguages")]
        public List<string> SupportedLanguages { get; set; }

        [JsonProperty("PackageIds")]
        public object PackageIds { get; set; }

        [JsonProperty("Markets")]
        public List<string> Markets { get; set; }
    }

    public partial class SkuLocalizedProperty
    {
        [JsonProperty("Contributors")]
        public List<object> Contributors { get; set; }

        [JsonProperty("Features")]
        public List<object> Features { get; set; }

        [JsonProperty("MinimumNotes")]
        public string MinimumNotes { get; set; }

        [JsonProperty("RecommendedNotes")]
        public string RecommendedNotes { get; set; }

        [JsonProperty("ReleaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty("DisplayPlatformProperties")]
        public object DisplayPlatformProperties { get; set; }

        [JsonProperty("SkuDescription")]
        public string SkuDescription { get; set; }

        [JsonProperty("SkuTitle")]
        public string SkuTitle { get; set; }

        [JsonProperty("SkuButtonTitle")]
        public string SkuButtonTitle { get; set; }

        [JsonProperty("DeliveryDateOverlay")]
        public object DeliveryDateOverlay { get; set; }

        [JsonProperty("SkuDisplayRank")]
        public List<object> SkuDisplayRank { get; set; }

        [JsonProperty("TextResources")]
        public object TextResources { get; set; }

        [JsonProperty("Images")]
        public List<object> Images { get; set; }

        [JsonProperty("LegalText")]
        public LegalText LegalText { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Markets")]
        public List<string> Markets { get; set; }
    }

    public partial class LegalText
    {
        [JsonProperty("AdditionalLicenseTerms")]
        public string AdditionalLicenseTerms { get; set; }

        [JsonProperty("Copyright")]
        public string Copyright { get; set; }

        [JsonProperty("CopyrightUri")]
        public string CopyrightUri { get; set; }

        [JsonProperty("PrivacyPolicy")]
        public string PrivacyPolicy { get; set; }

        [JsonProperty("PrivacyPolicyUri")]
        public string PrivacyPolicyUri { get; set; }

        [JsonProperty("Tou")]
        public string Tou { get; set; }

        [JsonProperty("TouUri")]
        public string TouUri { get; set; }
    }

    public partial class Availability
    {
        [JsonProperty("Actions")]
        public List<string> Actions { get; set; }

        [JsonProperty("AvailabilityASchema")]
        public string AvailabilityASchema { get; set; }

        [JsonProperty("AvailabilityBSchema")]
        public string AvailabilityBSchema { get; set; }

        [JsonProperty("AvailabilityId")]
        public string AvailabilityId { get; set; }

        [JsonProperty("Conditions")]
        public Conditions Conditions { get; set; }

        [JsonProperty("LastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("Markets")]
        public List<string> Markets { get; set; }

        [JsonProperty("OrderManagementData")]
        public OrderManagementData OrderManagementData { get; set; }

        [JsonProperty("Properties")]
        public AvailabilityProperties Properties { get; set; }

        [JsonProperty("SkuId")]
        public string SkuId { get; set; }

        [JsonProperty("DisplayRank")]
        public long DisplayRank { get; set; }

        [JsonProperty("RemediationRequired")]
        public bool RemediationRequired { get; set; }
    }

    public partial class AvailabilityProperties
    {
        [JsonProperty("OriginalReleaseDate")]
        public DateTime? OriginalReleaseDate { get; set; }
    }

    public partial class OrderManagementData
    {
        [JsonProperty("GrantedEntitlementKeys")]
        public List<object> GrantedEntitlementKeys { get; set; }

        [JsonProperty("PIFilter")]
        public PiFilter PiFilter { get; set; }

        [JsonProperty("Price")]
        public Price Price { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("CurrencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("IsPIRequired")]
        public bool IsPiRequired { get; set; }

        [JsonProperty("ListPrice")]
        public long ListPrice { get; set; }

        [JsonProperty("MSRP")]
        public long Msrp { get; set; }

        [JsonProperty("TaxType")]
        public string TaxType { get; set; }

        [JsonProperty("WholesaleCurrencyCode")]
        public string WholesaleCurrencyCode { get; set; }
    }

    public partial class PiFilter
    {
        [JsonProperty("ExclusionProperties")]
        public List<object> ExclusionProperties { get; set; }

        [JsonProperty("InclusionProperties")]
        public List<object> InclusionProperties { get; set; }
    }

    public partial class Conditions
    {
        [JsonProperty("ClientConditions")]
        public ClientConditions ClientConditions { get; set; }

        [JsonProperty("EndDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("ResourceSetIds")]
        public List<string> ResourceSetIds { get; set; }

        [JsonProperty("StartDate")]
        public DateTime StartDate { get; set; }
    }

    public partial class ClientConditions
    {
        [JsonProperty("AllowedPlatforms")]
        public List<AllowedPlatform> AllowedPlatforms { get; set; }
    }

    public partial class AllowedPlatform
    {
        [JsonProperty("MaxVersion")]
        public long MaxVersion { get; set; }

        [JsonProperty("MinVersion")]
        public long MinVersion { get; set; }

        [JsonProperty("PlatformName")]
        public string PlatformName { get; set; }
    }

    public partial class AlternateId
    {
        [JsonProperty("IdType")]
        public string IdType { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }

    public partial class DisplayCatalogModel
    {
        public static DisplayCatalogModel FromJson(string json) => JsonConvert.DeserializeObject<DisplayCatalogModel>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this DisplayCatalogModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
