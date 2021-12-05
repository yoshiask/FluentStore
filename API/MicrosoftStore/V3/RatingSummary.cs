namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class RatingSummary
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int Star1Count { get; set; }
        public int Star2Count { get; set; }
        public int Star3Count { get; set; }
        public int Star4Count { get; set; }
        public int Star5Count { get; set; }
        public int Star1ReviewCount { get; set; }
        public int Star2ReviewCount { get; set; }
        public int Star3ReviewCount { get; set; }
        public int Star4ReviewCount { get; set; }
        public int Star5ReviewCount { get; set; }
        public Review MostCriticalReview { get; set; }
        public Review MostFavorableReview { get; set; }
    }
}
