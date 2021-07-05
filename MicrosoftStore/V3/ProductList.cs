using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ProductListPayload
    {
        public string ListType { get; set; }
        public string ListId { get; set; }
        public string Anid { get; set; }
        public string Title { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public string AlgoName { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public List<ProductSummary> Products { get; set; }
    }
}
