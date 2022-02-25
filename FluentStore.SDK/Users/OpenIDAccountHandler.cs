using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;
using FluentStore.Services;
using OwlCore.AbstractUI.Models;
using System.Collections.Generic;
using Flurl;
using IdentityModel.OidcClient;
using System.Security.Claims;
using System;

namespace FluentStore.SDK.Users
{
    public abstract class OpenIDAccountHandler<TAccount> : AccountHandlerBase<TAccount> where TAccount : Account
    {
        public override abstract HashSet<string> HandledNamespaces { get; }

        protected string Token { get; private set; }
        protected string RefreshToken { get; private set; }

        protected abstract string Authority { get; }
        protected abstract string ClientId { get; }
        protected abstract string ClientSecret { get; }
        protected abstract string[] Scopes { get; }

        private OidcClient _client;
        private AuthorizeState _state;

        /// <summary>
        /// Called when sign-in is successful. <see cref="AccountHandlerBase.CurrentUser"/>
        /// is expected to be populated after this method is executed.
        /// </summary>
        protected abstract Task OnSignInSuccess();

        /// <summary>
        /// Called when sign-out is successful.
        /// </summary>
        protected abstract Task OnSignOut();

        public override Task<bool> SignInAsync(CredentialBase credential) => SignInAsync(null, credential.Password);

        /// <summary>
        /// Sign in using the supplied token and refresh token.
        /// </summary>
        /// <remarks>
        /// If <paramref name="token"/> is <see langword="null"/>,
        /// then <paramref name="refreshToken"/> will be used to
        /// get a new token.
        /// </remarks>
        /// <returns>Whether the sign-in succeeded.</returns>
        protected async Task<bool> SignInAsync(string token, string refreshToken)
        {
            try
            {
                if (token == null)
                {
                    // Use refresh token to get a new token
                    var resp = await _client.RefreshTokenAsync(refreshToken);
                    if (resp != null)
                    {
                        Token = resp.AccessToken;
                        RefreshToken = resp.RefreshToken;
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(resp);
#endif
                    }
                }

                await OnSignInSuccess();

                SaveCredential(RefreshToken);

                IsLoggedIn = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex.Message);
#endif

                Token = null;
                RefreshToken = null;
                IsLoggedIn = false;
            }

            return IsLoggedIn;
        }

        public override async Task SignOutAsync()
        {
            await OnSignOut();

            RemoveCredential(RefreshToken);

            IsLoggedIn = false;
            _client = null;
            _state = null;
            Token = RefreshToken = null;
            CurrentUser = null;
        }

        public override AbstractUICollection CreateSignInUI()
        {
            AbstractButton signInButton = new("SignInButton", "Sign in with browser", type: AbstractButtonType.Confirm);
            signInButton.Clicked += async (sender, e) =>
            {
                // Generate start URL, state, nonce, code challenge
                RefreshClient();
                _state = await _client.PrepareLoginAsync();

                INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
                await navService.OpenInBrowser(_state.StartUrl);
            };

            AbstractUICollection ui = new("SignInCollection")
            {
                Items = new AbstractUIElement[]
                {
                    signInButton
                }
            };
            return ui;
        }

        public override async Task HandleAuthActivation(Url url)
        {
            var result = await _client.ProcessResponseAsync(url.ToString(), _state);
            await SignInAsync(result.AccessToken, result.RefreshToken);
        }

        public async Task<IEnumerable<Claim>> GetOpenIDClaims()
        {
            var result = await _client.GetUserInfoAsync(Token);
            return result.Claims;
        }

        private void RefreshClient()
        {
            OidcClientOptions options = new()
            {
                Authority = Authority,
                ClientId = ClientId,
                RedirectUri = $"fluentstore://auth/{GetDefaultNamespcace()}",
                Scope = string.Join(' ', Scopes),
            };
            options.Policy.Discovery.ValidateIssuerName = false;
            options.Policy.Discovery.ValidateEndpoints = false;

            _client = new(options);
        }
    }
}
