using FluentStoreAPI.Models.Firebase;
using Flurl.Http;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public static class FlurlHttpExceptionEx
    {
        public static async Task<ErrorResponse> GetErrorResponse(this FlurlHttpException ex)
        {
            var obj = await ex.GetResponseJsonAsync<Newtonsoft.Json.Linq.JObject>();
            return obj["error"].ToObject<ErrorResponse>();
        }
    }
}
