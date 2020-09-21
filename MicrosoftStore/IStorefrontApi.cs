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
        Task<Recommendations> GetHomeRecommendations(string market = "US", string locale = "en-US", string appversion = "11905.1001.0.0", int pageSize = 15);
    }
}
