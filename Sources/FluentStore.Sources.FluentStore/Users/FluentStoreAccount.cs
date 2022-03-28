using FluentStore.SDK.Users;
using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Firebase;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccount : Account
    {
        public FluentStoreAccount(User user = null, Profile profile = null)
        {
            if (user != null)
                Update(user);
            if (profile != null)
                Update(profile);
        }

        public void Update(User user)
        {
            Id = user.LocalID;
            DisplayName = user.DisplayName;
            Email = user.Email;
        }

        public void Update(Profile profile)
        {
            DisplayName = profile.DisplayName;
        }
    }
}
