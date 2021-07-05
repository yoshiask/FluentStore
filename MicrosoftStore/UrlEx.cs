using Flurl;
using System.Globalization;

namespace MicrosoftStore
{
    internal static class UrlEx
    {
        public static Url SetMarketAndLocale(this Url url, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;
            RegionInfo region = new RegionInfo(culture.LCID);

            return url.SetQueryParam("market", region.TwoLetterISORegionName).SetQueryParam("locale", culture.Name);
        }

        public static Url GetBase(this Url url, CultureInfo culture = null)
        {
            return url.SetMarketAndLocale(culture).SetQueryParam("appVersion", Constants.APP_VERSION);
        }

        public static Url GetStorefrontBase(CultureInfo culture = null, double version = 9.0)
        {
            return Constants.STOREFRONT_API_HOST.AppendPathSegment("v" + version.ToString("0.0")).GetBase(culture);
        }
    }
}
