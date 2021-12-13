using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class Markets
    {
        public List<string> AllowedMarkets { get; set; }

        public bool HasMarket(CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            RegionInfo region = new(culture.LCID);
            return HasMarket(region);
        }

        public bool HasMarket(RegionInfo region)
        {
            return AllowedMarkets.Contains(region.TwoLetterISORegionName);
        }
    }
}