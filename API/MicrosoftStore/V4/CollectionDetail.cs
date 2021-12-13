using System;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V4
{
    public class CollectionDetail
    {
        public string AlgoName { get; set; }
        public string AlgoValue { get; set; }
        public List<V8.One.CardModel> Cards { get; set; }
        public string CollectionItemType { get; set; }
        public string CuratedBGColor { get; set; }
        public string CuratedDescription { get; set; }
        public string CuratedFGColor { get; set; }
        public Uri CuratedImageUrl { get; set; }
        public string CuratedTitle { get; set; }
        public string Id { get; set; }
        public string MediaType { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
    }
}
