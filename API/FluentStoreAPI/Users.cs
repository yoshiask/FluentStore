using FluentStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStoreAPI;

public partial class FluentStoreApiClient
{
    public async Task SignUpAndCreateProfileAsync(string email, string password, Profile profile)
    {
        await SignUpAsync(email, password);
        await UpdateUserProfileAsync(profile);
    }

    public async Task<UserInformation?> GetCurrentUserInformationAsync()
    {
        var user = _supabase.Auth.CurrentUser;

        if (user is null || user.Id is null)
            return null;

        var uid = new Guid(user.Id);
        UserInformation userInformation = new()
        {
            Uid = uid,
            Email = user.Email,
            FirebaseId = user.Id,
        };

        var profile = await GetProfileAsync(uid);
        if (profile is null)
            return userInformation;

        if (userInformation.DisplayName is null)
        {
            string? displayName = null;
            if (user.UserMetadata.TryGetValue("display_name", out var savedDisplayName)
                || user.UserMetadata.TryGetValue("name", out savedDisplayName)
                || user.UserMetadata.TryGetValue("full_name", out savedDisplayName))
                displayName = savedDisplayName?.ToString();

            userInformation.DisplayName = displayName ?? user.Email;
        }

        return userInformation;
    }

    public async Task<Profile?> GetProfileAsync(Guid id, CancellationToken token = default)
    {
        var profileResponse = await _supabase.From<Profile>()
            .Where(p => p.Uid == id)
            .Get(token);

        if (profileResponse.Model is not null && profileResponse.Model.DisplayName is null)
            profileResponse.Model.DisplayName = id.ToString();

        return profileResponse.Model;
    }

    public async Task<bool> UpdateUserProfileAsync(Profile profile, CancellationToken token = default)
    {
        if (profile.CreatedAt.Ticks == 0)
            profile.CreatedAt = DateTimeOffset.Now;

        profile.ModifiedAt = DateTimeOffset.Now;

        var response = await _supabase.From<Profile>().Upsert(profile, cancellationToken: token);
        return response.Model is not null;
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
