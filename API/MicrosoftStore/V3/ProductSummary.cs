using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ProductSummary
    {
        public List<V2.ImageItem> Images { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public bool IsUniversal { get; set; }
        public double Price { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public bool HasFreeTrial { get; set; }
        public string ProductType { get; set; }
        public string ProductFamilyName { get; set; }
        public string MediaType { get; set; }
        public string CollectionItemType { get; set; }
        public int NumberOfSeasons { get; set; }
        public long DurationInSeconds { get; set; }
        public bool DeveloperOptOfOfSDCardInstall { get; set; }
        public bool HasAddOns { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public string AutosuggestSubtitle { get; set; }
        public bool HideFromCollections { get; set; }
        public bool HideFromDownloadsAndUpdates { get; set; }
        public bool GamingOptionsXboxLive { get; set; }
    }
}
