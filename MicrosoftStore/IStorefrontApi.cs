using Refit;
using System.Threading.Tasks;
using MicrosoftStore.Responses;
using System.Collections.Generic;
using MicrosoftStore.Models;

namespace MicrosoftStore
{
    public interface IStorefrontApi
    {
        /// <summary>
        /// Get all the page details for the given product
        /// </summary>
        [Get("/pages/pdp")]
        Task<List<ResponseItem>> GetPage(string productId, string market, string locale, string appversion);


        /// <summary>
        /// Gets the details for the product with the given product ID
        /// </summary>
        [Get("/products/{productId}")]
        Task<ResponseItem> GetProduct(string productId, string market, string locale);

        /// <summary>
        /// Gets trending recommendation cards of home page
        /// </summary>
        [Get("/recommendations/collections/Collection/TrendingHomeColl1?market={market}&locale={locale}&deviceFamily=Windows.Desktop&appVersion={appversion}&pageSize={pageSize}&cardsEnabled=true")]
        Task<ResponseItem<RecommendationsPayload>> GetHomeRecommendations(string market = "US", string locale = "en-US", string appversion = "11905.1001.0.0", int pageSize = 15);

        /// <summary>
        /// Performs a search in the given locale for the query, and additionally filters by category, media type, and device family.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="market"></param>
        /// <param name="locale"></param>
        /// <param name="mediaType"></param>
        /// <param name="category"></param>
        /// <param name="deviceFamily"></param>
        /// <param name="pageSize"></param>
        /// <param name="skipItems"></param>
        /// <param name="appVersion"></param>
        /// <returns></returns>
        [Get("/search")]
        Task<ResponseItem<ProductCardListPayload>> Search(string query, string market, string locale, string mediaType, string category, string deviceFamily, int pageSize, int skipItems, string appVersion = "12011.1001.0.0");
    }
}
