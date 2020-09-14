using Refit;
using System.Threading.Tasks;
using MicrosoftStore.Responses;
using System.Collections.Generic;

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
    }
}
