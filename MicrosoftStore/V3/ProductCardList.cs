using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ProductCardList
    {
        public string AlgoName { get; set; }
        public string Anid { get; set; }
        public List<V8.One.CardModel> Cards { get; set; }
        public List<V1.ContentFilter> FilterOptions { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public string ListId { get; set; }
        public string ListType { get; set; }
        public int PageSize { get; set; }
        public string Title { get; set; }
        public int TotalItems { get; set; }
    }
}
