using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using FluentStoreAPI;
using FluentStoreAPI.Models;
using Flurl;
using OwlCore.AbstractUI.Models;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using System;
using System.Threading.Tasks;
using static Supabase.Gotrue.Constants;

namespace FluentStore.Sources.FluentStore.Users
{
    public class FluentStoreAccountHandler : AccountHandlerBase<FluentStoreAccount>
    {
        private readonly FluentStoreApiClient _client;

        public FluentStoreAccountHandler(FluentStoreApiClient client, IPasswordVaultService passwordVault, ICommonPathManager pathManager)
            : base(passwordVault)
        {
            _client = client;

            _client.SupabaseClient.Auth.AddStateChangedListener(OnSupabaseAuthStateChanged);
            _client.SupabaseClient.Auth.SetPersistence(new PasswordVaultSessionPersistence(passwordVault, pathManager, this));
        }

        public override string Id => "fluent-store-user";

        public override string DisplayName => "Fluent Store";

        public override Task HandleAuthActivation(Url url) => Task.CompletedTask;

        public override Task<bool> SignInAsync(CredentialBase credential) => Task.Run(SignIn);

        /// <summary>
        /// Sign in using the supplied token and refresh token.
        /// </summary>
        /// <remarks>
        /// If <paramref name="token"/> is <see langword="null"/>,
        /// then <paramref name="refreshToken"/> will be used to
        /// get a new token.
        /// </remarks>
        /// <returns>Whether the sign-in succeeded.</returns>
        protected bool SignIn()
        {
            try
            {
                if (_client.SupabaseClient.Auth.CurrentSession is null)
                {
                    // Attempt to load previous session
                    _client.SupabaseClient.Auth.LoadSession();
                }
            }
            catch
            {
                IsLoggedIn = false;
            }

            return IsLoggedIn;
        }

        public override async Task SignOutAsync() => await _client.SupabaseClient.Auth.SignOut();

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
            }
            catch (GotrueException ex)
            {
                string errorMessage = ex.Reason.ToString();

                epForm.EmailBox.Subtitle = errorMessage;

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

            Profile profile = new()
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

            Profile profile = new()
            {
                DisplayName = displayName
            };

            if (await _client.UpdateUserProfileAsync(profile))
            {
                await UpdateCurrentUser();
            }
        }

        private void OnSupabaseAuthStateChanged(IGotrueClient<Supabase.Gotrue.User, Supabase.Gotrue.Session> sender, AuthState stateChanged)
        {
            switch (stateChanged)
            {
                case AuthState.SignedIn:
                    IsLoggedIn = true;
                    break;

                case AuthState.SignedOut:
                    IsLoggedIn = false;
                    CurrentUser = null;
                    break;

                case AuthState.UserUpdated:
                    var profile = _client.GetCurrentUserProfile();

                    IsLoggedIn = profile is not null;
                    CurrentUser = IsLoggedIn
                        ? new FluentStoreAccount(profile)
                        : null;
                    break;
            }
        }
    }
}
