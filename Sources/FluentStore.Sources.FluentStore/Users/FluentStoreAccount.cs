using System;
using FluentStore.SDK.Users;
using FluentStoreAPI.Models;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccount : Account
    {
        public FluentStoreAccount(Profile profile = null)
        {
            if (profile != null)
                Update(profile);
        }

        public void Update(Profile profile)
        {
            Uuid = profile.Id;
            Id = profile.Id.ToString();
            DisplayName = profile.DisplayName;
            Email = profile.Email;
        }

        public Guid Uuid { get; private set; }
    }
}
