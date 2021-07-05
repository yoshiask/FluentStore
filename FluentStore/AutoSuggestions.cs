using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.Models
{
    public class AutoSuggestions
    {
        public List<V3.ProductDetails> AssetSuggestions { get; set; }
        public List<string> SearchSuggestions { get; set; }
        public List<object> MerchandizingSuggestions { get; set; }
    }
}
