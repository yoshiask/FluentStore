﻿using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
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
            var fbProfile = await GetFirestoreBase().AppendPathSegments("users", userId, "public", "profile")
                .WithOAuthBearerToken(Token).GetJsonAsync<Newtonsoft.Json.Linq.JObject>();

            return fbProfile.ToObject<Models.Firebase.Document>().Transform<Profile>();
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, Profile profile)
        {
            return await UpdateUserDocument(userId, Models.Firebase.Document.Untransform(profile), "public", "profile");
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
            // Make sure collection has a unique ID
            if (collection.Id == Guid.Empty)
                collection.Id = Guid.NewGuid();

            // Set author to current user
            collection.AuthorId = userId;

            return await UpdateUserDocument(userId, "collections",
                collection.Id.ToString(), Models.Firebase.Document.Untransform(collection));
        }

        public async Task<bool> DeleteCollectionAsync(string userId, string collectionId)
        {
            return await DeleteUserDocument(userId, "collections", collectionId);
        }
    }
}
