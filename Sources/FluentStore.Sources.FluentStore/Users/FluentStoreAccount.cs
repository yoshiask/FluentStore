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
            Uuid = userInformation.Uid;
            Id = userInformation.Uid.ToString();
            DisplayName = userInformation.DisplayName;
            Email = userInformation.Email;
        }

        public Guid Uuid { get; private set; }
    }
}
