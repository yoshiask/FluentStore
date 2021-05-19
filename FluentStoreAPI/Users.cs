using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        private IFlurlRequest GetFirestoreBase()
        {
            return FIRESTORE_BASE_URL.WithTimeout(10);
        }

        public async Task<User> GetUserAsync(string userId)
        {
            var fbCollections = await GetFirestoreBase().AppendPathSegments("users", userId)
                .WithOAuthBearerToken(Token).GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            var doc = fbCollections.ToObject<Models.Firebase.Document>();

            return doc.Transform<User>();
        }

        public async Task<Profile> GetUserProfileAsync(string userId)
        {
            var fbProfile = await GetFirestoreBase().AppendPathSegments("users", userId, "public", "profile")
                .WithOAuthBearerToken(Token).GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            var doc = fbProfile.ToObject<Models.Firebase.Document>();

            return doc.Transform<Profile>();
        }

        public async Task<List<Collection>> GetCollectionsAsync(string userId)
        {
            var fbCollections = await GetFirestoreBase().AppendPathSegments("users", userId, "collections")
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
