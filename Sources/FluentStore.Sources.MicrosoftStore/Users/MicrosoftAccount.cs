using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.Users;
using FluentStore.Services;
using OwlCore.AbstractUI.Models;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccount : Account
    {
        protected override AbstractUICollection CreateManageAccountForm()
        {
            AbstractButton manageButton = new("manageButton", "Manage account", iconCode: "\uE8A7", type: AbstractButtonType.Confirm);
            manageButton.Clicked += async (sender, e) =>
            {
                INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
                await navService.OpenInBrowser("https://account.microsoft.com/profile");
            };

            AbstractUICollection ui = new("ManageCollection")
            {
                manageButton
            };
            return ui;
        }
    }
}
