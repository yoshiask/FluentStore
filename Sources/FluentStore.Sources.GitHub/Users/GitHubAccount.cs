using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Garfoot.Utilities.FluentUrn;
using OwlCore.AbstractUI.Models;
using System;
using System.Linq;

namespace FluentStore.Sources.GitHub.Users
{
    public class GitHubAccount : Account
    {
        private const string ABSUI_ID_NAMEBOX = "nameBox";
        private const string ABSUI_ID_BIOBOX = "bioBox";
        private const string ABSUI_ID_COMPANYBOX = "companyBox";
        private const string ABSUI_ID_LOCATIONBOX = "locationBox";

        public Octokit.User GitHubUser { get; set; }

        public GitHubAccount(Octokit.User user = null)
        {
            if (user != null)
                Update(user);
        }

        public void Update(Octokit.User user)
        {
            Urn = new(GitHubAccountHandler.NAMESPACE_GHUSER, new RawNamespaceSpecificString(user.Id.ToString()));
            DisplayName = user.Name;
            GitHubUser = user;
        }

        protected override AbstractUICollection CreateManageAccountForm()
        {
            return AbstractUIHelper.CreateSingleButtonUI("ManageCollection", "ManageButton", "Manage account", "\uE8A7",
                async (sender, e) =>
                {
                    INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
                    await navService.OpenInBrowser(GitHubUser.HtmlUrl);
                });

            // FIXME: The update call returns HTTP 404
            AbstractButton manageButton = new("manageButton", "Save", iconCode: "\uE74E", type: AbstractButtonType.Confirm);
            manageButton.Clicked += ManageButton_Clicked;

            AbstractUICollection ui = new("ManageCollection")
            {
                new AbstractTextBox(ABSUI_ID_NAMEBOX, DisplayName, "Name"),
                new AbstractTextBox(ABSUI_ID_BIOBOX, GitHubUser.Bio, "Bio"),
                new AbstractTextBox(ABSUI_ID_COMPANYBOX, GitHubUser.Company, "Company"),
                new AbstractTextBox(ABSUI_ID_LOCATIONBOX, GitHubUser.Location, "Location"),
                manageButton
            };
            return ui;
        }

        private async void ManageButton_Clicked(object sender, EventArgs e)
        {
            Octokit.UserUpdate update = new();

            foreach (AbstractTextBox box in ManageAccountForm.Where(elem => elem is AbstractTextBox))
            {
                string val = box.Value;
                switch (box.Id)
                {
                    case ABSUI_ID_NAMEBOX:
                        update.Name = val;
                        break;

                    case ABSUI_ID_BIOBOX:
                        update.Bio = val;
                        break;

                    case ABSUI_ID_COMPANYBOX:
                        update.Company = val;
                        break;

                    case ABSUI_ID_LOCATIONBOX:
                        update.Location = val;
                        break;
                }
            }

            _ = await GitHubHandler.GetClient().User.Update(update);
        }
    }
}
