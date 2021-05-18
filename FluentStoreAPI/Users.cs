using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        private Url GetFirebaseBase()
        {
            return FIRESTORE_BASE_URL.SetQueryParam("key", KEY);
        }

        public async Task<User> GetCurrentFSUserAsync()
        {
            return await STORAGE_BASE_URL.AppendPathSegment("HomePage.json")
                .SetQueryParam("alt", "media").GetJsonAsync<User>();
        }

        public async Task<List<Collection>> GetCollectionsAsync(string userId)
        {
            var fbCollections = await FIRESTORE_BASE_URL.AppendPathSegments("users", userId, "collections")
                .WithOAuthBearerToken(Token).GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            var documents = fbCollections["documents"].ToObject<List<Models.Firebase.Document>>();

            var collections = new List<Collection>(documents.Count);
            foreach (Models.Firebase.Document doc in documents)
            {
                collections.Add(doc.Transform<Collection>());
            }
            return collections;
        }
    }
}
