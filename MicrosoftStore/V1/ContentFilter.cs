using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V1
{
    public class ContentFilter
    {
        public List<ContentFilterChoice> Choices { get; set; }
        public string DefaultChoiceId { get; set; }
        public string FilterId { get; set; }
        public string InitialChoiceId { get; set; }
        public bool IsChoiceRequired { get; set; }
        public bool IsHidden { get; set; }
        public bool KeepExpanded { get; set; }
        public bool MultiSelectionEnabled { get; set; }
        public string Title { get; set; }
    }
}
