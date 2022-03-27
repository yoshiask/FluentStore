using FluentStore.Services;
using Flurl;
using OwlCore.AbstractUI.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FluentStore.SDK.Users
{
    public abstract class AccountHandlerBase
    {
        private readonly IPasswordVaultService _passwordVaultService;

        public AccountHandlerBase(IPasswordVaultService passwordVaultService)
        {
            _passwordVaultService = passwordVaultService;
        }

        /// <summary>
        /// A unique identifier for this type of account handler.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// The display name of this handler.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Whether this account handler is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The currently signed in user. <see langword="null"/> if <see cref="IsLoggedIn"/> is <see langword="false"/>.
        /// </summary>
        public Account CurrentUser { get; protected set; }

        /// <summary>
        /// Whether a user is signed in.
        /// </summary>
        public bool IsLoggedIn { get; protected set; }

        /// <summary>
        /// If the user is not already signed in, attempt to silently sign in
        /// using credentials saved by <see cref="IPasswordVaultService"/>.
        /// </summary>
        public virtual async Task<bool> TrySilentSignInAsync()
        {
            if (IsLoggedIn) return true;

            try
            {
                if (TryGetAllCredentials(out var credentials))
                {
                    bool success = await SignInAsync(credentials[0]);
                    return success;
                }
            } catch { }

            return false;
        }

        /// <summary>
        /// Creates a <see cref="Url"/> that can be used as a redirect URI for authentication.
        /// <para>
        /// When the app is activated using this URI, <see cref="HandleAuthActivation(Url)"/>
        /// will be called with the URI.
        /// </para>
        /// </summary>
        public virtual Url GetAuthProtocolUrl() => $"fluentstore://auth/{Id}";

        /// <summary>
        /// Determines if <paramref name="otherUser"/> is the signed in user.
        /// </summary>
        /// <returns>
        /// Whether <paramref name="otherUser"/> is equal to <see cref="CurrentUser"/>.
        /// <para>
        /// <see langword="false"/> when <see cref="IsLoggedIn"/> is <see langword="false"/>.
        /// </para>
        /// </returns>
        public virtual Task<bool> IsCurrentUser(Account otherUser) => Task.FromResult(IsLoggedIn && otherUser == CurrentUser);

        /// <summary>
        /// Saves the credentials of the current user to the password vault.
        /// </summary>
        /// <param name="password">
        /// The password to associate with <see cref="CurrentUser"/>.
        /// <para>
        /// Note that this does not have to be an actual password,
        /// for example it may be a refresh token or any other string
        /// that can be used to silently sign in.
        /// </para>
        /// </param>
        public virtual void SaveCredential(string password)
        {
            if (_passwordVaultService != null)
                _passwordVaultService.Add(CreateCredential(password));
        }

        /// <summary>
        /// Removes the credentials of the current user from the password vault.
        /// </summary>
        /// <param name="password">
        /// The password associated with <see cref="CurrentUser"/>.
        /// </param>
        public virtual void RemoveCredential(string password)
        {
            if (_passwordVaultService != null)
                _passwordVaultService.Remove(CreateCredential(password));
        }

        /// <summary>
        /// Retrieves the credentials of the user with the specified ID from the password vault.
        /// </summary>
        /// <param name="userUrn">
        /// The URN of the user to get credentials for.
        /// </param>
        /// <returns>
        /// The first credential in the password vault that matches the <paramref name="userUrn"/>.
        /// <para>
        /// <see langword="null"/> if no credential exists.
        /// </para>
        /// </returns>
        public virtual CredentialBase GetCredential(string userId)
        {
            CredentialBase credential = null;

            if (_passwordVaultService != null)
            {
                credential = GetAllCredentials()
                    .FirstOrDefault(c => c.UserName == userId);
            }

            return credential;
        }

        /// <summary>
        /// Attempts to retrieve the credentials of the user with the specified ID from the password vault.
        /// </summary>
        /// <param name="userUrn">
        /// The URN of the user to get credentials for.
        /// </param>
        /// <param name="credential">
        /// <see langword="null"/> if no credential exists.
        /// </param>
        /// <returns>
        /// <see langword="false"/> if no credential exists.
        /// </returns>
        public bool TryGetCredential(string userId, [NotNullWhen(true)] out CredentialBase credential)
        {
            credential = GetCredential(userId);
            return credential != null;
        }

        /// <summary>
        /// Retrieves all credentials in the password vault with the
        /// specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to look up.</param>
        /// <returns>A list of matching <see cref="CredentialBase"/>s.</returns>
        public virtual IList<CredentialBase> GetAllCredentials()
        {
            return _passwordVaultService.FindAllByResource(GetAuthProtocolUrl());
        }

        /// <summary>
        /// Attempts to retrieve all credentials in the password vault with the
        /// specified namespace.
        /// </summary>
        /// <param name="ns">
        /// The namespace to look up.
        /// </param>
        /// <param name="credentials">
        /// A list of matching <see cref="CredentialBase"/>s.
        /// </param>
        /// <returns>
        /// <see langword="false"/> if no credentials are matched.
        /// </returns>
        /// <remarks>
        /// If <see langword="true"/> is returned, then <paramref name="credentials"/> is
        /// guarenteed to be not <see langword="null"/> and have at least one element.
        /// </remarks>
        public bool TryGetAllCredentials([NotNullWhen(true)] out IList<CredentialBase> credentials)
        {
            try
            {
                credentials = GetAllCredentials();
                return credentials != null && credentials.Count > 0;
            }
            catch
            {
                credentials = null;
                return false;
            }
        }

        /// <summary>
        /// Sign in using the supplied <see cref="CredentialBase"/>. Typically called by <see cref="TrySilentSignInAsync"/>.
        /// </summary>
        public abstract Task<bool> SignInAsync(CredentialBase credential);

        /// <summary>
        /// Clears the state of this handler.
        /// </summary>
        public abstract Task SignOutAsync();

        /// <summary>
        /// Completes the sign-in process after the app is activated.
        /// Often used for OAuth2 flows.
        /// </summary>
        /// <param name="url">
        /// The <see cref="Url"/> the app was activated with.
        /// </param>
        public abstract Task HandleAuthActivation(Url url);

        /// <summary>
        /// Gets the <see cref="AbstractUICollection"/> that represents a sign-in form.
        /// </summary>
        protected abstract AbstractUICollection CreateSignInForm();

        /// <summary>
        /// Gets the <see cref="AbstractUICollection"/> that represents an account creation form.
        /// </summary>
        protected abstract AbstractUICollection CreateSignUpForm();

        /// <summary>
        /// Gets an updated <see cref="CurrentUser"/> after a successful sign-in.
        /// </summary>
        /// <remarks>
        protected abstract Task<Account> UpdateCurrentUser();

        /// <summary>
        /// Creates an empty credential for the <see cref="CurrentUser"/>.
        /// </summary>
        /// <returns>
        /// A credential that can be used to save and retrieve
        /// data from <see cref="IPasswordVaultService"/>.
        /// </returns>
        private CredentialBase CreateCredential(string password)
        {
            return new(userName: CurrentUser.Id, password: password, resource: GetAuthProtocolUrl());
        }
    }

    public abstract class AccountHandlerBase<TAccount> : AccountHandlerBase where TAccount : Account
    {
        public AccountHandlerBase(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
        }

        /// <summary>
        /// Casts <see cref="AccountHandlerBase.CurrentUser"/> to <typeparamref name="TAccount"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TAccount GetAccount() => (TAccount)CurrentUser;

        /// <summary>
        /// Casts <see cref="AccountHandlerBase.UpdateCurrentUser"/> to <typeparamref name="TAccount"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected async Task<TAccount> UpdateAccount() => (TAccount)await UpdateCurrentUser();
    }
}
