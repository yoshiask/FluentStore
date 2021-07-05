using Flurl;
using Flurl.Http;
using MicrosoftStore.Responses;
using MicrosoftStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using static MicrosoftStore.UrlEx;

namespace MicrosoftStore
{
    public class StorefrontApi
    {
        /// <summary>
        /// Get all the page details for the given product.
        /// </summary>
        public async Task<List<ResponseItem>> GetPage(string productId, CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("pages", "pdp")
                .SetQueryParam("productId", productId).GetJsonAsync<List<ResponseItem>>();
        }

        /// <summary>
        /// Gets the details for the product with the given product ID.
        /// </summary>
        public async Task<ResponseItem<ProductDetails>> GetProduct(string productId, CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("products", productId)
                .GetJsonAsync<ResponseItem<ProductDetails>>();
        }

        /// <summary>
        /// Gets trending recommendation cards of home page.
        /// </summary>
        public async Task<ResponseItem<RecommendationsPayload>> GetHomeRecommendations(int pageSize = 15, CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("recommendations", "collections", "Collection", "TrendingHomeColl1")
                .SetQueryParam("cardsEnabled", true).SetQueryParam("deviceFamily", "Windows.Desktop")
                .GetJsonAsync<ResponseItem<RecommendationsPayload>>();
        }

        /// <summary>
        /// Performs a search in the given locale for the query, and additionally filters by category, media type, and device family.
        /// </summary>
        public async Task<ResponseItem<SearchPayload>> Search(string query, string mediaType = "all", string deviceFamily = "Windows.Desktop", CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegments("search")
                .SetQueryParam("query", query).SetQueryParam("mediaType", mediaType).SetQueryParam("deviceFamily", deviceFamily)
                .SetQueryParam("cardsEnabled", true)
                .GetJsonAsync<ResponseItem<SearchPayload>>();
        }

        /// <summary>
        /// Gets a list of search suggestions for a given query fragment.
        /// </summary>
        public async Task<ResponseItem<AutoSuggestionsPayload>> GetSearchSuggestions(string query, string deviceFamily = "Windows.Desktop", CultureInfo culture = null)
        {
            return await GetStorefrontBase(culture).AppendPathSegment("autosuggest")
                .SetQueryParam("prefix", query).SetQueryParam("deviceFamily", deviceFamily)
                .GetJsonAsync<ResponseItem<AutoSuggestionsPayload>>();
        }
    }
}
