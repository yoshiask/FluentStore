using Flurl.Http;
using System.Threading.Tasks;
using static Microsoft.Marketplace.Storefront.Contracts.UrlEx;
using static Microsoft.Marketplace.Storefront.Contracts.Constants;
using Newtonsoft.Json;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Microsoft.Marketplace.Storefront.Contracts.V1;

namespace Microsoft.Marketplace.Storefront.Contracts
{
    public class StorefrontApi
    {
        /// <summary>
        /// Get all the page details for the given product.
        /// </summary>
        public async Task<ResponseItemList> GetPage(string productId, CatalogIdType idType, RequestOptions options = null)
        {
            string json = await GetStorefrontBase(options).AppendPathSegments("pages", "pdp")
                .SetQueryParam("productId", productId).SetQueryParam("idType", idType)
                .GetStringAsync();
            return JsonConvert.DeserializeObject<ResponseItemList>(json, DefaultJsonSettings);
        }

        /// <summary>
        /// Gets the details for the product with the given product ID.
        /// </summary>
        public async Task<ResponseItem<V3.ProductDetails>> GetProduct(string productId, CatalogIdType idType, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("products", productId)
                .SetQueryParam("idType", idType)
                .GetJsonAsync<ResponseItem<V3.ProductDetails>>();
        }

        /// <summary>
        /// Gets trending recommendation cards of home page.
        /// </summary>
        public async Task<ResponseItem<V4.CollectionDetail>> GetHomeRecommendations(int pageSize = 15, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("recommendations", "collections", "Collection", "TrendingHomeColl1")
                .SetQueryParam("cardsEnabled", true)
                .GetJsonAsync<ResponseItem<V4.CollectionDetail>>();
        }

        /// <summary>
        /// Gets the cards displayed at the top of the Home page in the Microsoft Store Preview app.
        /// </summary>
        public async Task<ResponseItem<V4.CollectionDetail>> GetHomeSpotlight(int pageSize = 15, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("ems", "curated", "HomeSpotlight")
                .SetQueryParam("cardsEnabled", true)
                .SetQueryParam("placementId", 10837389)
                .GetJsonAsync<ResponseItem<V4.CollectionDetail>>();
        }

        /// <summary>
        /// Performs a search in the given locale for the query, and additionally filters by category, media type, and device family.
        /// </summary>
        public async Task<ResponseItem<V9.SearchResponse>> Search(string query, string mediaType = "all", RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("search")
                .SetQueryParam("query", query).SetQueryParam("mediaType", mediaType)
                .SetQueryParam("cardsEnabled", true)
                .GetJsonAsync<ResponseItem<V9.SearchResponse>>();
        }

        /// <summary>
        /// Gets the next page of the given search response.
        /// </summary>
        public async Task<ResponseItem<V9.SearchResponse>> NextSearchPage(V9.SearchResponse currentResponse)
        {
            return await (STOREFRONT_API_HOST + currentResponse.NextUri)
                .GetJsonAsync<ResponseItem<V9.SearchResponse>>();
        }

        /// <summary>
        /// Gets a list of search suggestions for a given query fragment.
        /// </summary>
        public async Task<ResponseItem<V3.AutoSuggestions>> GetSearchSuggestions(string query, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegment("autosuggest")
                .SetQueryParam("prefix", query)
                .GetJsonAsync<ResponseItem<V3.AutoSuggestions>>();
        }
    }
}
