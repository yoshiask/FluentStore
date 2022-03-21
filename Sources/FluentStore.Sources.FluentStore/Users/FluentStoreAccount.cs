using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.Users;
using FluentStoreAPI.Models.Firebase;
using Garfoot.Utilities.FluentUrn;
using OwlCore.AbstractUI.Models;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccount : Account
    {
        public FluentStoreAccount(User user = null)
        {
            if (user != null)
                Update(user);
        }

        public void Update(User user)
        {
            Urn = new(FluentStoreAccountHandler.NAMESPACE_FSUSER, new RawNamespaceSpecificString(user.LocalID));
            DisplayName = user.DisplayName;
            Email = user.Email;
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
