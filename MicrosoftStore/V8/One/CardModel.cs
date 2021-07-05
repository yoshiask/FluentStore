using Microsoft.Marketplace.Storefront.Contracts.Enums;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V8.One
{
    public class CardModel
    {
        public string ProductId { get; set; }
        public TileLayout TileLayout { get; set; }
        public string Title { get; set; }
        public List<V2.ImageItem> Images { get; set; }
        public string DisplayPrice { get; set; }
        public double Price { get; set; }
        public double AverageRating { get; set; }
        public string RatingsCount { get; set; }
        public List<string> PackageFamilyNames { get; set; }
        public List<string> ContentIds { get; set; }
        public bool GamingOptionsXboxLive { get; set; }
        public string AvailableDevicesDisplayText { get; set; }
        public string AvailableDevicesNarratorText { get; set; }
        public string TypeTag { get; set; }
        public string LongDescription { get; set; }
        public string ProductFamilyName { get; set; }
        public bool IsGamingAppOnly { get; set; }
        public List<string> Categories { get; set; }
        public string Schema { get; set; }

        public int RatingCount => int.Parse(RatingsCount);
    }
}
