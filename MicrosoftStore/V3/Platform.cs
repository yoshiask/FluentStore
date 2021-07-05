using Microsoft.Marketplace.Storefront.Contracts.Enums;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class Platform
    {
        public PlatWindows PlatformName { get; set; }
        public long MaxTested { get; set; }
        public long MinVersion { get; set; }
    }
}
