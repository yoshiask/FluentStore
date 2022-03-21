using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Graph;
using OwlCore.AbstractUI.Models;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccount : Account
    {
        public MicrosoftAccount(User user = null)
        {
            if (user != null)
                Update(user);
        }

        public void Update(User user)
        {
            Urn = new(MicrosoftAccountHandler.NAMESPACE_MSACCOUNT, new RawNamespaceSpecificString(user.Id));
            DisplayName = user.DisplayName;
            Email = user.Mail;
        }

        protected override AbstractUICollection CreateManageAccountForm()
        {
            return AbstractUIHelper.CreateSingleButtonUI("ManageCollection", "ManageButton", "Manage account", "\uE8A7",
                async (sender, e) =>
                {
                    INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
                    await navService.OpenInBrowser("https://account.microsoft.com/profile");
                });
        }
    }
}
