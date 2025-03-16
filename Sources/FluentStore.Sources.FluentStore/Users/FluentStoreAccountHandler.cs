using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using FluentStoreAPI;
using FluentStoreAPI.Models;
using Flurl;
using OwlCore.AbstractUI.Models;
using Supabase.Gotrue.Exceptions;
using System;
using System.Threading.Tasks;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccountHandler : AccountHandlerBase<FluentStoreAccount>
    {
        private readonly FluentStoreAPI.FluentStoreAPI _client;

        public FluentStoreAccountHandler(FluentStoreAPI.FluentStoreAPI fsApi, IPasswordVaultService passwordVault)
            : base(passwordVault)
        {
            _client = fsApi;
        }

        public override string Id => "fluent-store-user";

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
                // TODO: Sign in

                IsLoggedIn = true;
            }
            catch (Exception ex)
            {
                IsLoggedIn = false;
            }

            return IsLoggedIn;
        }

        public override Task SignOutAsync()
        {
            // TODO: Sign out

            return Task.CompletedTask;
        }

        public override AbstractForm CreateSignInForm()
        {
            return new EmailPasswordForm($"{Id}_SignInForm", "Sign in", OnSignInFormSubmitted);
        }

        public override AbstractForm CreateSignUpForm()
        {
            EmailPasswordForm form = new($"{Id}_SignUpForm", "Sign up", OnSignUpFormSubmitted);

            // Add display name box
            AbstractTextBox displayNameBox = new($"{form.Id}_DisplayName", null, "Display name");
            form.Add(displayNameBox);

            return form;
        }

        public override AbstractForm CreateManageAccountForm()
        {
            AbstractForm form = new($"{Id}_ManageForm", onSubmit: OnManageAccountFormSubmitted);

            // Add display name box
            AbstractTextBox displayNameBox = new($"{form.Id}_DisplayName", null, "Display name");
            form.Add(displayNameBox);

            return form;
        }

        protected override async Task<Account> UpdateCurrentUser()
        {
            var profile = await _client.GetCurrentUserProfileAsync();
            return new FluentStoreAccount(profile);
        }

        private async void OnSignInFormSubmitted(object sender, EventArgs e)
        {
            if (sender is not EmailPasswordForm epForm)
                return;

            string email = epForm.GetEmail();
            string password = epForm.GetPassword();

            try
            {
                await _client.SignInAsync(email, password);
                //await SignInAsync(response);
            }
            catch (GotrueException ex)
            {
                string errorMessage = ex.Reason.ToString();

                AbstractTextBox errorMessageBox = epForm.GetChildById<AbstractTextBox>("ErrorMessageBox");
                if (errorMessageBox is null)
                {
                    errorMessageBox = new("ErrorMessageBox", errorMessage);
                    epForm.Add(errorMessageBox);
                }
                else
                {
                    errorMessageBox.Value = errorMessage;
                }
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                // TODO: Error messages

                string errorMessage = ex.Message;

                AbstractTextBox errorMessageBox = epForm.GetChildById<AbstractTextBox>("ErrorMessageBox");
                if (errorMessageBox is null)
                {
                    errorMessageBox = new("ErrorMessageBox", errorMessage);
                    epForm.Add(errorMessageBox);
                }
                else
                {
                    errorMessageBox.Value = errorMessage;
                }
            }
        }

        private async void OnSignUpFormSubmitted(object sender, EventArgs e)
        {
            if (sender is not EmailPasswordForm epForm)
                return;

            string email = epForm.GetEmail();
            string password = epForm.GetPassword();
            string displayName = epForm.GetChildById<AbstractTextBox>($"{epForm.Id}_DisplayName")?.Value;

            FluentStoreAPI.Models.Profile profile = new()
            {
                DisplayName = displayName
            };

            await _client.SignUpAndCreateProfileAsync(email, password, profile);
        }

        private async void OnManageAccountFormSubmitted(object sender, EventArgs e)
        {
            if (sender is not AbstractForm form)
                return;

            string displayName = form.GetChildById<AbstractTextBox>($"{form.Id}_DisplayName")?.Value;

            FluentStoreAPI.Models.Profile profile = new()
            {
                DisplayName = displayName
            };

            if (await _client.UpdateUserProfileAsync(profile))
            {
                await UpdateCurrentUser();
            }
        }
    }
}
