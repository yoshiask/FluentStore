using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class Review
    {
        public string ProductId { get; set; }
        public string ReviewerName { get; set; }
        public double Rating { get; set; }
        public string Market { get; set; }
        public string Locale { get; set; }
        public int HelpfulPositive { get; set; }
        public int HelpfulNegative { get; set; }
        public Guid ReviewId { get; set; }
        public string ReviewText { get; set; }
        public string Title { get; set; }
        public DateTimeOffset SubmittedDateTimeUtc { get; set; }
        public bool IsProductTrial { get; set; }
        public bool IsTakenDown { get; set; }
        public bool ViolationsFound { get; set; }
        public bool IsPublished { get; set; }
        public bool IsRevised { get; set; }
        public bool UpdatedSinceResponse { get; set; }
        public bool IsAppInstalled { get; set; }
        public PlatWindows DeviceFamily { get; set; }

        [JsonIgnore]
        public bool IsValid => ReviewId != Guid.Empty;
    }
}
