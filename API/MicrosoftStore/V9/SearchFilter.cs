using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V9
{
    public class SearchFilter
    {
        public string Id { get; set; }
        public bool AlwaysVisible { get; set; }
        public string Title { get; set; }
        public List<string> DependentFilters { get; set; }
        public List<SearchFilterChoice> Choices { get; set; }
    }
}
