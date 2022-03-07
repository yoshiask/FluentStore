using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Flurl;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Sources.GitHub.Users
{
    public class GitHubAccountHandler : AccountHandlerBase<GitHubAccount>
    {
        public const string NAMESPACE_GHUSER = "gh-user";

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_GHUSER,
        };

        public override string DisplayName => "GitHub";

        private string Token { get; set; }

        private readonly string[] _scopes = new[]
        {
            "read:user", "user:email", "repo",
        };

        protected override async Task<Account> UpdateCurrentUser()
        {
            var user = await GitHubHandler.GetClient().User.Current();
            return new GitHubAccount(user);
        }

        public override Task<bool> SignInAsync(CredentialBase credential) => SignInAsync(credential.Password);

        public async Task<bool> SignInAsync(string token)
        {
            try
            {
                Token = token;

                CredentialStore.Current.Token = Token;
                CurrentUser = await UpdateCurrentUser();

                SaveCredential(Token);

                IsLoggedIn = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex.Message);
#endif

                Token = null;
                IsLoggedIn = false;
            }

            return IsLoggedIn;
        }

        public override Task SignOutAsync()
        {
            CredentialStore.Current.Token = null;

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
            var response = await GitHubHandler.GetClient().Oauth.CreateAccessToken(request);

            await SignInAsync(response.AccessToken);
        }

        protected override AbstractUICollection CreateSignInForm()
        {
            return AbstractUIHelper.CreateSingleButtonUI("SignInCollection", "SignInButton", "Sign in with browser", "\uE8A7",
                async (sender, e) =>
                {
                    // Generate start URL
                    // https://docs.github.com/en/developers/apps/building-oauth-apps/authorizing-oauth-apps#1-request-a-users-github-identity
                    Octokit.OauthLoginRequest request = new(Secrets.GH_CLIENTID)
                    {
                        RedirectUri = GetAuthProtocolUrl(null).ToUri()
                    };
                    foreach (string scope in _scopes)
                        request.Scopes.Add(scope);
                    var uri = GitHubHandler.GetClient().Oauth.GetGitHubLoginUrl(request);

                    //Url url = "https://github.com/login/oauth/authorize"
                    //    .SetQueryParam("client_id", Secrets.GH_CLIENTID)
                    //    .SetQueryParam("redirect_uri", GetAuthProtocolUrl(null))
                    //    .SetQueryParam("scope", string.Join(' ', _scopes));

                    INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
                    await navService.OpenInBrowser(uri);
                });
        }

        protected override AbstractUICollection CreateSignUpForm()
        {
            throw new NotImplementedException();
        }
    }
}
