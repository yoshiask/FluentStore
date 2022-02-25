using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using OwlCore.AbstractUI.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.SDK.Users
{
    public delegate void OnLoginStateChangedHandler(bool isLoggedIn);

    public abstract class AccountHandlerBase : ObservableObject
    {
        /// <summary>
        /// A list of all namespaces this handler can handle.
        /// </summary>
        /// <remarks>
        /// Namespaces cannot be shared across handlers.
        /// </remarks>
        public abstract HashSet<string> HandledNamespaces { get; }

        private Account _currentUser;
        private bool _isLoggedIn;
        private readonly IPasswordVaultService _passwordVaultService = Ioc.Default.GetService<IPasswordVaultService>();

        /// <summary>
        /// The currently signed in user. <see langword="null"/> if <see cref="IsLoggedIn"/> is <see langword="false"/>.
        /// </summary>
        public Account CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        /// <summary>
        /// Whether a user is signed in.
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }

        /// <summary>
        /// If the user is not already signed in, attempt to silently sign in
        /// using credentials saved by <see cref="IPasswordVaultService"/>.
        /// </summary>
        public virtual async Task<bool> TrySilentSignInAsync()
        {
            if (IsLoggedIn) return true;

            try
            {
                string urnPrefix = "urn:" + GetDefaultNamespcace();
                var loginCredential = _passwordVaultService.FindAllByResource(CredentialBase.DEFAULT_RESOURCE)
                    .FirstOrDefault(c => c.UserName.StartsWith(urnPrefix));

                if (loginCredential != null)
                {
                    bool success = await SignInAsync(loginCredential);
                    return success;
                }
            } catch { }

            return false;
        }

        /// <summary>
        /// Gets the namespace this handler uses by default for accounts.
        /// </summary>
        public virtual string GetDefaultNamespcace() => HandledNamespaces.First();

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
        public virtual CredentialBase GetCredential(Urn userUrn)
        {
            CredentialBase credential = null;

            if (_passwordVaultService != null)
            {
                string userName = userUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;
                credential = _passwordVaultService.FindAllByResource(userUrn.NamespaceIdentifier)
                    .FirstOrDefault(c => c.UserName == userName);
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
        public bool TryGetCredential(Urn userUrn, [NotNullWhen(true)] out CredentialBase credential)
        {
            credential = GetCredential(userUrn);
            return credential != null;
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
        /// Gets the <see cref="AbstractUICollection"/> that represents a sign-in form.
        /// </summary>
        public abstract AbstractUICollection CreateSignInUI();

        /// <summary>
        /// Completes the sign-in process after the app is activated.
        /// Often used for OAuth2 flows.
        /// </summary>
        /// <param name="url">
        /// The <see cref="Url"/> the app was activated with.
        /// </param>
        public abstract Task HandleAuthActivation(Url url);

        /// <summary>
        /// Creates an empty credential for the <see cref="CurrentUser"/>.
        /// </summary>
        /// <returns>
        /// A credential that can be used to save and retrieve
        /// data from <see cref="IPasswordVaultService"/>.
        /// </returns>
        private CredentialBase CreateCredential(string password)
        {
            string userName = CurrentUser.Urn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            string resource = CurrentUser.Urn.NamespaceIdentifier;
            return new(userName, password, resource);
        }
    }

    public abstract class AccountHandlerBase<TAccount> : AccountHandlerBase where TAccount : Account
    {
        private TAccount _currentUser;
        public new TAccount CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }
    }
}
