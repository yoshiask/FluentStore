using Flurl.Http;
using System.Threading.Tasks;
using static Microsoft.Marketplace.Storefront.Contracts.UrlEx;
using static Microsoft.Marketplace.Storefront.Contracts.Constants;
using Newtonsoft.Json;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Microsoft.Marketplace.Storefront.Contracts.V1;
using System.Collections.Generic;

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
        /// Gets a subset of the list of reviews for a product.
        /// </summary>
        public async Task<ResponseItem<V3.ReviewList>> GetProductReviews(string productId, int pageSize = 15, int skipItems = 0, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("ratings", "product", productId)
                .SetQueryParam("pageSize", pageSize).SetQueryParam("skipItems", skipItems)
                .GetJsonAsync<ResponseItem<V3.ReviewList>>();
        }

        /// <summary>
        /// Gets all product reviews by calling <see cref="GetProductReviews(string, int, int, RequestOptions)"/>
        /// untill all results are returned.
        /// </summary>
        public async IAsyncEnumerable<V3.Review> GetAllProductReviews(string productId, int pageSize = 25, int startAt = 0, RequestOptions options = null)
        {
            int recievedItems = startAt;
            int totalItems;
            do
            {
                var response = await GetProductReviews(productId, pageSize, recievedItems, options);
                var reviewList = response.Payload;

                totalItems = reviewList.TotalItems;
                recievedItems += reviewList.Reviews.Count;

                foreach (var review in reviewList.Reviews)
                    yield return review;
            } while (recievedItems < totalItems);
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
        public async Task<V3.ProductList> GetHomeSpotlight(int pageSize = 15, RequestOptions options = null)
        {
            var json = await GetStorefrontBase(options).AppendPathSegments("pages", "home")
                .SetQueryParam("cardsEnabled", true)
                .SetQueryParam("placementId", 10837389)
                .GetStringAsync();

            var responses = JsonConvert.DeserializeObject<ResponseItemList>(json, DefaultJsonSettings);
            return responses.GetPayload<V3.ProductList>();
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

        /// <summary>
        /// Gets a list of collections.
        /// </summary>
        public async Task<ResponseItem<V4.CollectionDetail>> GetCollections(int pageSize = 15, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("canvas", "listofcollections")
                .SetQueryParam("cardsEnabled", true)
                .SetQueryParam("pageSize", pageSize)
                .SetQueryParam("site", "Channels")
                .SetQueryParam("collectionId", "MerchandiserContent/Apps/Collection-C/CollectionCAppsPage")
                .GetJsonAsync<ResponseItem<V4.CollectionDetail>>();
        }

        /// <summary>
        /// Get all the details for the given collection.
        /// </summary>
        public async Task<ResponseItem<V4.CollectionDetail>> GetCollection(string collectionId, RequestOptions options = null)
        {
            return await GetStorefrontBase(options).AppendPathSegments("canvas", "collections")
                .SetQueryParam("site", "Channels")
                .SetQueryParam("collectionId", collectionId)
                .SetQueryParam("deviceFamilyVersion", 2814751208898560)  // NOTE: What is this? Without it, most of the cards are missing
                .SetQueryParam("cardsEnabled", true)
                .GetJsonAsync<ResponseItem<V4.CollectionDetail>>();
        }
    }
}
