using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public static class FluentStoreAPI
    {
        public const string STORAGE_BASE_URL = "https://firebasestorage.googleapis.com/v0/b/fluent-store.appspot.com/o/";

        public static async Task<HomePageFeatured> GetHomePageFeaturedAsync()
        {
            var json = await STORAGE_BASE_URL.AppendPathSegment("HomePage.json")
                    .SetQueryParam("alt", "media").GetStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<HomePageFeatured>(json);
        }
    }
}
