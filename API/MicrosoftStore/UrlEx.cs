using Flurl;
using Flurl.Http;
using System.Globalization;

namespace Microsoft.Marketplace.Storefront.Contracts
{
    internal static class UrlEx
    {
        public static IFlurlRequest SetMarketAndLocale(this Url url, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;
            RegionInfo region = new(culture.LCID);

            return url.SetQueryParam("market", region.TwoLetterISORegionName)
                      .SetQueryParam("locale", culture.Name)
                      .WithHeader("Accept-Language", culture.ToString());
        }

        public static IFlurlRequest GetBase(this Url url, CultureInfo culture = null)
        {
            return url.SetMarketAndLocale(culture).SetQueryParam("appVersion", Constants.APP_VERSION);
        }

        public static IFlurlRequest GetStorefrontBase(CultureInfo culture = null, double version = 9.0)
        {
            return Constants.STOREFRONT_API_HOST.AppendPathSegment("v" + version.ToString("0.0", CultureInfo.InvariantCulture)).GetBase(culture);
        }
    }
}
