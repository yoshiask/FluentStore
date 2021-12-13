using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class AutoSuggestions
    {
        public List<ProductSummary> AssetSuggestions { get; set; }
        public List<string> SearchSuggestions { get; set; }
        public List<object> MerchandizingSuggestions { get; set; }
    }
}
