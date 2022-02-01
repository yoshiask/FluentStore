using Flurl;
using Microsoft.Marketplace.Storefront.Contracts;
using System.Globalization;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic
{
    internal static class UrlEx
    {
        public static Url SetMarket(this Url url, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;
            RegionInfo region = new(culture.LCID);

            return url.SetMarket(region);
        }
        public static Url SetMarket(this Url url, RegionInfo region)
        {
            return url.SetQueryParam("Market", region.TwoLetterISORegionName);
        }

        public static Url GetStoreEdgeFDBase(CultureInfo culture = null, double version = 9.0)
        {
            return Constants.STOREFRONT_API_HOST
                .AppendPathSegment("v" + version.ToString("0.0", CultureInfo.GetCultureInfo("en-001")))
                .SetMarket(culture);
        }
    }
}
