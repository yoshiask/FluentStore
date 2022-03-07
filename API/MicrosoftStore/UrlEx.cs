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

        public static IFlurlRequest GetStorefrontBase(RequestOptions options = default)
        {
            IFlurlRequest baseRequest = new Url(Constants.STOREFRONT_API_HOST)
                .AppendPathSegment("v" + options.Version.ToString("0.0", CultureInfo.GetCultureInfo("en-001")))
                .SetMarketAndLocale(options.Culture)
                .SetQueryParam("appVersion", Constants.APP_VERSION)
                .SetQueryParam("deviceFamily", options.DeviceFamily)
                .SetQueryParam("architecture", options.DeviceArchitecture);

            if (options.Token != null)
            {
                baseRequest = baseRequest.WithOAuthBearerToken(options.Token);
            }

            return baseRequest;
        }
    }
}
