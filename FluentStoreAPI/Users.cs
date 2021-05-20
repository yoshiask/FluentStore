using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
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

            return fbProfile.ToObject<Models.Firebase.Document>().Transform<Profile>();
        }

        public async Task<List<Collection>> GetCollectionsAsync(string userId)
        {
            var documents = await GetUserBucket(userId, "collections");
            var collections = new List<Collection>(documents.Count);
            foreach (Models.Firebase.Document doc in documents)
            {
                collections.Add(doc.Transform<Collection>());
            }
            return collections;
        }

        public async Task<Collection> GetCollectionAsync(string userId, string collectionId)
        {
            var document = await GetUserDocument(userId, "collections", collectionId);
            return document.Transform<Collection>();
        }

        public async Task<bool> UpdateCollectionAsync(string userId, Collection collection)
        {
            return await UpdateUserDocument(userId, "collections",
                collection.Id.ToString(), Models.Firebase.Document.Untransform(collection));
        }
    }
}
