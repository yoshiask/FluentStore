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
        Task<List<StorefrontResponseItem>> GetPage(string productId, string market, string locale, string appversion);
    }
}
