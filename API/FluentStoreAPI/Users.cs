﻿using FluentStoreAPI.Models;
using Flurl;
using Flurl.Http;
using Google.Apis.Firestore.v1.Data;
using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        public async Task SignUpAndCreateProfileAsync(string email, string password, Profile profile)
        {
            await SignUpAsync(email, password);
            await UpdateDisplayNameAsync(profile.DisplayName);
        }

        public async Task<Profile> GetUserProfileAsync() => await Task.Run(GetUserProfile);

        public Profile GetUserProfile()
        {
            Profile profile = new();

            if (_supabase.Auth.CurrentUser!.UserMetadata.TryGetValue("display_name", out var displayName))
                profile.DisplayName = displayName?.ToString() ?? _supabase.Auth.CurrentUser.Email!;

            return profile;
        }

        public async Task<bool> UpdateUserProfileAsync(Profile profile)
        {
            await UpdateDisplayNameAsync(profile.DisplayName);
            return true;
        }

        public async Task<List<Collection>> GetCollectionsAsync(Guid userId)
        {
            var response = await _supabase.From<Collection>()
                .Where(c => c.AuthorId == userId)
                .Get();

            return response.Models;
        }

        public async Task<Collection> GetCollectionAsync(Guid collectionId)
        {
            var response = await _supabase.From<Collection>()
                .Where(c => c.Id == collectionId)
                .Get();

            return response.Model
                ?? throw new Exception($"No collection with ID '{collectionId}' exists.");
        }

        public async Task<Guid?> UpdateCollectionAsync(Collection collection, CancellationToken token = default)
        {
            // Make sure collection has a unique ID and updated timestamp
            if (collection.Id == Guid.Empty)
            {
                collection.Id = Guid.NewGuid();
                collection.CreatedAt = DateTimeOffset.Now;
            }

            // Set author to current user
            collection.AuthorId = new(_supabase.Auth.CurrentUser!.Id!);

            // Update modified timestamp
            collection.ModifiedAt = DateTimeOffset.Now;

            var response = await _supabase.From<Collection>().Upsert(collection, cancellationToken: token);
            return response.Model?.Id;
        }

        public async Task DeleteCollectionAsync(Guid collectionId, CancellationToken token = default)
        {
            await _supabase.From<Collection>()
                .Where(c => c.Id == collectionId)
                .Delete(cancellationToken: token);
        }
    }
}
