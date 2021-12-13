using Microsoft.Marketplace.Storefront.Contracts.Enums;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ConditionPlatform
    {
        public int MaxVersion { get; set; }
        public int MinVersion { get; set; }
        public PlatWindows PlatformName { get; set; }
    }
}
