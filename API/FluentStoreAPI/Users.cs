using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using Google.Apis.Firestore.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        public async Task<Models.Firebase.UserSignInResponse> SignUpAndCreateProfileAsync(string email, string password, Profile profile)
        {
            var signInResponse = await SignUpAsync(email, password);
            await UpdateUserProfileAsync(signInResponse.LocalID, profile);
            return signInResponse;
        }

        public async Task<Profile> GetUserProfileAsync(string userId)
        {
            var profile = await GetUserDocument(userId, "public", "profile");
            return new(profile);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, Profile profile)
        {
            return await UpdateUserDocument(userId, "public", "profile", profile);
        }

        public async Task<List<Collection>> GetCollectionsAsync(string userId)
        {
            var queryRequest = Documents.RunQuery(new RunQueryRequest()
            {
                StructuredQuery = new()
                {
                    From = [new CollectionSelector() { CollectionId = "collections" }],
                    //Where = new FieldFilter()
                }
            }, NAME_PREFIX + $"/users/{userId}");
            var queryResults = await queryRequest.ExecuteAsync();

            var request = Documents.ListDocuments(NAME_PREFIX, $"users/{userId}/collections");
            request.AccessToken = Token;
            var collections = await request.ExecuteAsync();

            return collections.Documents
                .Select(d => new Collection(d))
                .ToList();
        }

        public async Task<Collection> GetCollectionAsync(string userId, string collectionId)
        {
            var document = await GetUserDocument(userId, "collections", collectionId);
            return new Collection(document);
        }

        public async Task<bool> UpdateCollectionAsync(string userId, Collection collection, CancellationToken token = default)
        {
            // Make sure collection has a unique ID
            if (collection.Id == Guid.Empty)
                collection.Id = Guid.NewGuid();

            // Set author to current user
            collection.AuthorId = userId;

            return await UpdateUserDocument(userId, "collections",
                collection.Id.ToString(), collection, token);
        }

        public async Task<bool> DeleteCollectionAsync(string userId, string collectionId)
        {
            return await DeleteUserDocument(userId, "collections", collectionId);
        }
    }
}
