using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Flurl;
using Microsoft.Extensions.Logging;
using Octokit;
using OwlCore.AbstractUI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.Sources.GitHub.Users
{
    public class GitHubAccountHandler(GitHubClient client, CredentialStore credentialStore, IPasswordVaultService passwordVaultService, ILogger log)
        : AccountHandlerBase<GitHubAccount>(passwordVaultService)
    {
        private const string ABSUI_ID_NAMEBOX = "nameBox";
        private const string ABSUI_ID_BIOBOX = "bioBox";
        private const string ABSUI_ID_COMPANYBOX = "companyBox";
        private const string ABSUI_ID_LOCATIONBOX = "locationBox";

        public override string Id => "gh-user";

        public override string DisplayName => "GitHub";

        private string Token { get; set; }

        private readonly string[] _scopes = ["read:user", "user:email", "repo"];

        protected override async Task<SDK.Users.Account> UpdateCurrentUser()
        {
            var user = await client.User.Current();
            return new GitHubAccount(user);
        }

        public override Task<bool> SignInAsync(CredentialBase credential) => SignInAsync(credential.Password);

        public async Task<bool> SignInAsync(string token)
        {
            try
            {
                Token = token;

                credentialStore.Token = Token;
                CurrentUser = await UpdateCurrentUser();

                SaveCredential(Token);

                IsLoggedIn = true;
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex, "Failed to sign in to GitHub");

                Token = null;
                IsLoggedIn = false;
            }

            return IsLoggedIn;
        }

        public override Task SignOutAsync()
        {
            credentialStore.Token = null;

            RemoveCredential(Token);

            IsLoggedIn = false;
            Token = null;
            CurrentUser = null;

            return Task.CompletedTask;
        }

        public override async Task HandleAuthActivation(Url url)
        {
            if (!url.QueryParams.TryGetFirst("code", out var code))
                ThrowHelper.ThrowInvalidOperationException("No OAuth code was supplied.");

            Octokit.OauthTokenRequest request = new(Secrets.GH_CLIENTID, Secrets.GH_CLIENTSECRET, code.ToString());
            var response = await client.Oauth.CreateAccessToken(request);

            await SignInAsync(response.AccessToken);
        }

        public override AbstractForm CreateSignInForm()
        {
            return AbstractUIHelper.CreateSingleButtonForm("SignInCollection", "Click the button below to sign in with browser.", "Sign in",
                async (sender, e) =>
                {
                    // Generate start URL
                    // https://docs.github.com/en/developers/apps/building-oauth-apps/authorizing-oauth-apps#1-request-a-users-github-identity
                    OauthLoginRequest request = new(Secrets.GH_CLIENTID)
                    {
                        RedirectUri = GetAuthProtocolUrl().ToUri()
                    };
                    foreach (string scope in _scopes)
                        request.Scopes.Add(scope);
                    var uri = client.Oauth.GetGitHubLoginUrl(request);

                    NavigationServiceBase navService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
                    await navService.OpenInBrowser(uri);
                });
        }

        public override AbstractForm CreateSignUpForm()
        {
            throw new NotImplementedException();
        }

        public override AbstractForm CreateManageAccountForm()
        {
            return AbstractUIHelper.CreateOpenInBrowserForm("ManageCollection", "Manage your account on the website.",
                GetAccount().GitHubUser.HtmlUrl, Ioc.Default.GetRequiredService<NavigationServiceBase>());

            // FIXME: The update call returns HTTP 404
            AbstractForm form = new("ManageCollection", onSubmit: ManageButton_Clicked)
            {
                new AbstractTextBox(ABSUI_ID_NAMEBOX, DisplayName, "Name"),
                new AbstractTextBox(ABSUI_ID_BIOBOX, GetAccount().GitHubUser.Bio, "Bio"),
                new AbstractTextBox(ABSUI_ID_COMPANYBOX, GetAccount().GitHubUser.Company, "Company"),
                new AbstractTextBox(ABSUI_ID_LOCATIONBOX, GetAccount().GitHubUser.Location, "Location"),
            };
            return form;
        }

        private async void ManageButton_Clicked(object sender, EventArgs e)
        {
            if (sender is not AbstractForm form)
                return;

            UserUpdate update = new();

            foreach (AbstractTextBox box in form.Where(elem => elem is AbstractTextBox))
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

            _ = await client.User.Update(update);
        }
    }
}
