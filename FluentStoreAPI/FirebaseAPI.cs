using FluentStoreAPI.Models.Firebase;
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

        private async Task<Document> GetDocument(bool authenticate, params string[] path)
        {
            var request = GetFirestoreBase().AppendPathSegments(path);
            if (authenticate)
                request = request.WithOAuthBearerToken(Token);

            var document = await request.GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            return document.ToObject<Document>();
        }

        private async Task<Document> GetUserDocument(string userId, string bucket, string documentId)
        {
            return await GetDocument(true, "users", userId, bucket, documentId);
        }

        private async Task<Document> GetUserDocument(string userId, params string[] path)
        {
            var truePath = new string[path.Length + 2];
            truePath[0] = "users";
            truePath[1] = userId;
            System.Array.Copy(path, 0, truePath, 2, path.Length);
            return await GetDocument(true, path);
        }

        private async Task<bool> UpdateUserDocument(string userId, string bucket, string documentId, Document doc)
        {
            return await UpdateUserDocument(userId, doc, bucket, documentId);
        }

        private async Task<bool> UpdateUserDocument(string userId, Document doc, params string[] path)
        {
            var request = GetFirestoreBase().AppendPathSegments("users", userId).AppendPathSegments(path)
                .WithOAuthBearerToken(Token);
            var respose = await request.PatchJsonAsync(doc);
            return respose.ResponseMessage.IsSuccessStatusCode;
        }

        private async Task<bool> DeleteUserDocument(string userId, string bucket, string documentId)
        {
            return await DeleteUserDocument(userId, new[] { bucket, documentId });
        }

        private async Task<bool> DeleteUserDocument(string userId, params string[] path)
        {
            var request = GetFirestoreBase().AppendPathSegments("users", userId).AppendPathSegments(path)
                .WithOAuthBearerToken(Token);
            var respose = await request.DeleteAsync();
            return respose.ResponseMessage.IsSuccessStatusCode;
        }

        private async Task<List<Document>> GetUserBucket(string userId, string bucket)
        {
            var response = await GetFirestoreBase().AppendPathSegments("users", userId, bucket)
                .WithOAuthBearerToken(Token).GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            var docs = response["documents"];
            return docs != null ? docs.ToObject<List<Document>>() : new List<Document>();
        }
    }
}
