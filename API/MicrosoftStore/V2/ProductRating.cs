using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Marketplace.Storefront.Contracts.V2
{
    public class ProductRating
    {
        public string RatingSystem { get; set; }
        public string RatingSystemShortName { get; set; }
        public string RatingSystemId { get; set; }
        public string RatingSystemUrl { get; set; }
        public string RatingValue { get; set; }
        public string RatingId { get; set; }
        public string RatingValueLogoUrl { get; set; }
        public int RatingAge { get; set; }
        public bool RestrictMetadata { get; set; }
        public bool RestrictPurchase { get; set; }
        public List<string> RatingDescriptors { get; set; }
        public List<string> RatingDisclaimers { get; set; }
        public List<string> InteractiveElements { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public Uri RatingSystemUri
            => Uri.TryCreate(RatingSystemUrl, UriKind.Absolute, out var uri)
            ? uri : null;

        [JsonIgnore]
        public Uri RatingValueLogoUri
            => Uri.TryCreate(RatingValueLogoUrl, UriKind.Absolute, out var uri)
            ? uri : null;
    }
}
