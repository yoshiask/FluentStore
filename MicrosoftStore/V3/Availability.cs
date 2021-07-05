using System;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class Availability
    {
        public string AvailabilityId { get; set; }
        public double Price { get; set; }
        public string DisplayPrice { get; set; }
        public string StrikethroughPrice { get; set; }
        public string PromoMessage { get; set; }
        public bool RemediationRequired { get; set; }
        public DateTimeOffset AvailabilityEndDate { get; set; }
        public DateTimeOffset PreorderReleaseDate { get; set; }
        public int DisplayRank { get; set; }
        public AvailabilityConditions Conditions { get; set; }
        public List<string> Actions { get; set; }
        public bool IsGamesWithGold { get; set; }
    }
}
