using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using FluentStoreAPI;
using FluentStoreAPI.Models.Firebase;
using Flurl;
using OwlCore.AbstractUI.Models;
using System;
using System.Threading.Tasks;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccountHandler : AccountHandlerBase<FluentStoreAccount>
    {
        private readonly FluentStoreAPI.FluentStoreAPI FSApi;

        public FluentStoreAccountHandler(FluentStoreAPI.FluentStoreAPI fsApi, IPasswordVaultService passwordVault)
            : base(passwordVault)
        {
            FSApi = fsApi;
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

            try
            {
                var response = await FSApi.SignInAsync(email, password);
                await SignInAsync(response);
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                if (ex.StatusCode == 400)
                {
                    var errorResponse = await ex.GetErrorResponse();
                    string errorMessage = UserSignInResponse.CommonErrors.GetMessage(errorResponse.Message);

                    AbstractTextBox? errorMessageBox = epForm.GetChildById<AbstractTextBox>("ErrorMessageBox");
                    if (errorMessageBox == null)
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

            var response = await FSApi.SignUpAndCreateProfileAsync(email, password, profile);
            await SignInAsync(response);
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

            if (await FSApi.UpdateUserProfileAsync(CurrentUser.Id, profile))
            {
                await UpdateCurrentUser();
            }
        }
    }
}
