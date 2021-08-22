using Flurl.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using static Microsoft.Marketplace.Storefront.Contracts.UrlEx;
using Newtonsoft.Json;
using Microsoft.Marketplace.Storefront.Contracts.V1;

namespace Microsoft.Marketplace.Storefront.Contracts
{
    public class StorefrontApi
    {
        /// <summary>
        /// Get all the page details for the given product.
        /// </summary>
        public async Task<ResponseItemList> GetPage(string productId, CultureInfo culture = null)
        {
            string json = await GetStorefrontBase(culture).AppendPathSegments("pages", "pdp")
                .SetQueryParam("productId", productId).GetStringAsync();
            return JsonConvert.DeserializeObject<ResponseItemList>(json, Constants.DefaultJsonSettings);
        }

        /// <summary>
        /// Gets the details for the product with the given product ID.
        /// </summary>
        public async Task<ResponseItem<V3.ProductDetails>> GetProduct(string productId, CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("products", productId)
                .GetJsonAsync<ResponseItem<V3.ProductDetails>>();
        }

        /// <summary>
        /// Gets trending recommendation cards of home page.
        /// </summary>
        public async Task<ResponseItem<V4.CollectionDetail>> GetHomeRecommendations(int pageSize = 15, string deviceFamily = "Windows.Desktop", CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("recommendations", "collections", "Collection", "TrendingHomeColl1")
                .SetQueryParam("cardsEnabled", true).SetQueryParam("deviceFamily", deviceFamily)
                .GetJsonAsync<ResponseItem<V4.CollectionDetail>>();
        }

        /// <summary>
        /// Gets the cards displayed at the top of the Home page in the Microsoft Store Preview app.
        /// </summary>
        public async Task<ResponseItem<V4.CollectionDetail>> GetHomeSpotlight(int pageSize = 15, string deviceFamily = "Windows.Desktop", CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("ems", "curated", "HomeSpotlight")
                .SetQueryParam("cardsEnabled", true).SetQueryParam("deviceFamily", deviceFamily)
                .SetQueryParam("placementId", 10837389)
                .GetJsonAsync<ResponseItem<V4.CollectionDetail>>();
        }

        /// <summary>
        /// Performs a search in the given locale for the query, and additionally filters by category, media type, and device family.
        /// </summary>
        public async Task<ResponseItem<V9.SearchResponse>> Search(string query, string mediaType = "all", string deviceFamily = "Windows.Desktop", CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("search")
                .SetQueryParam("query", query).SetQueryParam("mediaType", mediaType).SetQueryParam("deviceFamily", deviceFamily)
                .SetQueryParam("cardsEnabled", true)
                .GetJsonAsync<ResponseItem<V9.SearchResponse>>();
        }

        /// <summary>
        /// Gets a list of search suggestions for a given query fragment.
        /// </summary>
        public async Task<ResponseItem<V3.AutoSuggestions>> GetSearchSuggestions(string query, string deviceFamily = "Windows.Desktop", CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegment("autosuggest")
                .SetQueryParam("prefix", query).SetQueryParam("deviceFamily", deviceFamily)
                .GetJsonAsync<ResponseItem<V3.AutoSuggestions>>();
        }
    }
}
