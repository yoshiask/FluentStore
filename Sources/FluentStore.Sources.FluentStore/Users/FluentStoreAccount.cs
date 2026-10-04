using System;
using FluentStore.SDK.Users;
using FluentStoreAPI.Models;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccount : Account
    {
        public FluentStoreAccount(UserInformation userInformation = null)
        {
            if (userInformation != null)
                Update(userInformation);
        }

        public void Update(UserInformation userInformation)
        {
            Profile = userInformation.Profile;

            Id = Profile.Uid.ToString();
            DisplayName = Profile.DisplayName;
            Email = userInformation.Email;
        }

        public Guid Uuid => Profile?.Uid ?? Guid.Empty;

        internal Profile Profile { get; private set; }
    }
}
