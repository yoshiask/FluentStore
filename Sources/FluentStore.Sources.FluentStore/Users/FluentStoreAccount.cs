using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.Users;
using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Firebase;
using Garfoot.Utilities.FluentUrn;
using OwlCore.AbstractUI.Models;

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
            Urn = new(FluentStoreAccountHandler.NAMESPACE_FSUSER, new RawNamespaceSpecificString(user.LocalID));
            DisplayName = user.DisplayName;
            Email = user.Email;
        }

        public void Update(Profile profile)
        {
            DisplayName = profile.DisplayName;
        }

        protected override AbstractUICollection CreateManageAccountForm()
        {
            return AbstractUIHelper.CreateSingleButtonUI("ManageCollection", "ManageButton", "Manage account", "\uE8A7",
                (sender, e) =>
                {
                    
                });
        }
    }
}
