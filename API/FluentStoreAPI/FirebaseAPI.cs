using Flurl.Http;
using Google.Apis.Firestore.v1.Data;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        private static string BuildName(params string[] path) => BuildName(string.Join("/", path));

        private static string BuildName(string name) => NAME_PREFIX + "/" + name;

        private static string BuildNameForUser(string userId, params string[] path)
        {
            var truePath = new string[path.Length + 2];
            truePath[0] = "users";
            truePath[1] = userId;
            System.Array.Copy(path, 0, truePath, 2, path.Length);
            return BuildName(truePath);
        }

        private static string BuildNameForUser(string userId, string name) => BuildName("users", userId, name);

        private async Task<Document> GetDocumentAsync(string name, CancellationToken token = default)
        {
            var documents = _firestore.Projects.Databases.Documents;

            var documentRequest = documents.Get(name);
            documentRequest.AccessToken = Token;

            return await documentRequest.ExecuteAsync(token);
        }

        private async Task<Document> GetUserDocument(string userId, string bucket, string documentId, CancellationToken token = default)
        {
            return await GetDocumentAsync(BuildName("users", userId, bucket, documentId), token);
        }

        private async Task<bool> UpdateUserDocument(string userId, string bucket, string documentId, Document doc, CancellationToken token = default)
        {
            return await UpdateUserDocument(userId, [bucket, documentId], doc, token);
        }

        private async Task<bool> UpdateUserDocument(string userId, string[] path, Document doc, CancellationToken token = default)
        {
            var request = Documents.Patch(doc, BuildNameForUser(userId, path));
            request.AccessToken = Token;
            return await FireRequestAsync(request);
        }

        private async Task<bool> DeleteUserDocument(string userId, string bucket, string documentId)
        {
            var request = Documents.Delete(BuildNameForUser(userId, bucket, documentId));
            request.AccessToken = Token;
            return await FireRequestAsync(request);
        }

        private async Task<bool> FireRequestAsync<TResponse>(Google.Apis.Requests.ClientServiceRequest<TResponse> request, CancellationToken token = default)
        {
            var httpRequest = request.CreateRequest();
            var response = await _firestore.HttpClient.SendAsync(httpRequest, token);
            return response.IsSuccessStatusCode;
        }
    }
}
