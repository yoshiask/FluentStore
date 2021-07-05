using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V9
{
    public class SearchResponse
    {
        public List<V8.One.CardModel> SearchResults { get; set; }
        public List<SearchFilter> FilterOptions { get; set; }
        public SearchFilter DepartmentOptions { get; set; }
        public string NextUri { get; set; }
    }
}
