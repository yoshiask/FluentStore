using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Flurl;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSAPI = FluentStoreAPI.FluentStoreAPI;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccountHandler : AccountHandlerBase<FluentStoreAccount>
    {
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();

        public const string NAMESPACE_FSUSER = "fluent-store-user";

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_FSUSER
        };

        public override string DisplayName => "Fluent Store";

        public override Task HandleAuthActivation(Url url) => Task.CompletedTask;

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
                FSApi.Token = token;
                FSApi.RefreshToken = refreshToken;

                if (FSApi.Token == null)
                {
                    // Use refresh token to get a new token
                    // This call internally sets the Token and RefreshToken props
                    await FSApi.UseRefreshToken();
                }

                CurrentUser = await UpdateAccount();

                if (FSApi.RefreshToken != null)
                    SaveCredential(FSApi.RefreshToken);

                IsLoggedIn = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(ex.Message);
#endif

                FSApi.Token = null;
                FSApi.RefreshToken = null;
                IsLoggedIn = false;
            }

            return IsLoggedIn;
        }

        protected async Task<bool> SignInAsync(FluentStoreAPI.Models.Firebase.UserSignInResponse signInResponse)
        {
            return await SignInAsync(signInResponse.IDToken, signInResponse.RefreshToken);
        }

        public override Task SignOutAsync()
        {
            RemoveCredential(FSApi.RefreshToken);

            IsLoggedIn = false;
            FSApi.Token = FSApi.RefreshToken = null;
            CurrentUser = null;

            return Task.CompletedTask;
        }

        protected override AbstractUICollection CreateSignInForm()
        {
            EmailPasswordForm form = new("SignInForm", OnSignInFormSubmitted);
            return form;
        }

        protected override AbstractUICollection CreateSignUpForm()
        {
            throw new NotImplementedException();
        }

        protected override async Task<Account> UpdateCurrentUser()
        {
            var user = (await FSApi.GetCurrentUserDataAsync())[0];
            var profile = await FSApi.GetUserProfileAsync(user.LocalID);

            return new FluentStoreAccount(user, profile);
        }

        private async void OnSignInFormSubmitted(object sender, EventArgs e)
        {
            if (sender is not EmailPasswordForm epForm)
                return;

            string email = epForm.GetEmail();
            string password = epForm.GetPassword();

            var response = await FSApi.SignInAsync(email, password);
            await SignInAsync(response);
        }
    }
}
