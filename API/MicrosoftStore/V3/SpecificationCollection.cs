using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class SpecificationCollection
    {
        public string Title { get; set; }
        public List<SpecificationItem> Items { get; set; }
    }
}
