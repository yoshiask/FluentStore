using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ProductList : ListBase
    {
        public string Anid { get; set; }
        public string Title { get; set; }
        public bool HasThirdPartyIAPs { get; set; }
        public string AlgoName { get; set; }
        public List<ProductSummary> Products { get; set; }

        // This is here because the API doesn't always use
        // ProductCardList when the payload is a list of cards.
        public List<V8.One.CardModel> Cards { get; set; }
    }
}
