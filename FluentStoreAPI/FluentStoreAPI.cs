using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Firebase;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public class FluentStoreAPI
    {
        public const string STORAGE_BASE_URL = "https://firebasestorage.googleapis.com/v0/b/fluent-store.appspot.com/o/";
        public const string IDENTITY_TK_BASE_URL = "https://identitytoolkit.googleapis.com/v1";

        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public FluentStoreAPI() { }
        public FluentStoreAPI(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }

        private Url GetIdentityTKBase()
        {
            return IDENTITY_TK_BASE_URL.SetQueryParam("key", "AIzaSyCoINaQk7QdzPryW0oZHppWnboRRPk26fQ");
        }

        public async Task<HomePageFeatured> GetHomePageFeaturedAsync()
        {
            return await STORAGE_BASE_URL.AppendPathSegment("HomePage.json")
                .SetQueryParam("alt", "media").GetJsonAsync<HomePageFeatured>();
        }

        public async Task<UserSignInResponse> SignUpAsync(string email, string password)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:signUp")
                .SendJsonAsync(HttpMethod.Post, new { email = email, password = password, returnSecureToken = true });
            return await response.GetJsonAsync<UserSignInResponse>();
        }

        public async Task<UserSignInResponse> SignInAsync(string email, string password)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:signInWithPassword")
                .SendJsonAsync(HttpMethod.Post, new { email = email, password = password, returnSecureToken = true });
            return await response.GetJsonAsync<UserSignInResponse>();
        }

        /// <summary>
        /// Signs in a user using an OAuth token.
        /// </summary>
        /// <param name="oauthData">Contains the OAuth credential (an ID token or access token)
        /// and provider ID which issues the credential. (e.g. id_token={idToken}&providerId={providerId})</param>
        /// <param name="returnIdpCredential">Whether to force the return of the OAuth
        /// credential on the following errors: <see cref="OAuthUserSignInResponse.CommonErrors.FEDERATED_USER_ID_ALREADY_LINKED"/>
        /// and <see cref="OAuthUserSignInResponse.CommonErrors.EMAIL_EXISTS"/>.</param>
        public async Task<OAuthUserSignInResponse> SignInWithOAuthAsync(string oauthData, bool returnIdpCredential = false)
        {
            var payload = new
            {
                // The URI to which the IDP redirects the user back.
                requestUri = "fluent-store://firebase_auth",
                postBody = oauthData,
                returnSecureToken = true,
                returnIdpCredential = returnIdpCredential
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:signInWithIdp")
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<OAuthUserSignInResponse>();
        }

        /// <summary>
        /// Lists all providers associated with a specified email.
        /// </summary>
        public async Task<ProvidersResponse> GetProvidersForEmailAsync(string email)
        {
            var payload = new
            {
                // The URI to which the IDP redirects the user back.
                // For this use case, this is just the current URL.
                continueUri = "fluent-store://firebase_auth",
                identifier = email
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:createAuthUri")
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<ProvidersResponse>();
        }

        /// <summary>
        /// Sends a password reset email.
        /// </summary>
        /// <returns>The user's email.</returns>
        public async Task<string> RequestPasswordResetAsync(string email)
        {
            var payload = new PasswordResetPayload()
            {
                Email = email
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:sendOobCode")
                .WithHeader("X-Firebase-Locale", CultureInfo.CurrentUICulture)
                .SendJsonAsync(HttpMethod.Post, payload);
            return (await response.GetJsonAsync()).email;
        }

        public async Task<PasswordResetPayload> VerifyPasswordResetAsync(string code)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:resetPassword")
                .SendJsonAsync(HttpMethod.Post, new { oobCode = code });
            return await response.GetJsonAsync<PasswordResetPayload>();
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="code">The email action code sent to the user's email for resetting the password.</param>
        /// <param name="newPassword">The user's new password.</param>
        public async Task<PasswordResetPayload> ConfirmPasswordResetAsync(string code, string newPassword)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:resetPassword")
                .SendJsonAsync(HttpMethod.Post, new { oobCode = code, newPassword = newPassword });
            return await response.GetJsonAsync<PasswordResetPayload>();
        }
        
        public async Task<UpdateAccountResponse> ChangeEmailAsync(string newEmail)
        {
            var payload = new
            {
                idToken = Token,
                email = newEmail,
                returnSecureToken = true
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .WithHeader("X-Firebase-Locale", CultureInfo.CurrentUICulture)
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<UpdateAccountResponse>();
        }

        public async Task<UpdateAccountResponse> ChangePasswordAsync(string newPassword)
        {
            var payload = new
            {
                idToken = Token,
                password = newPassword,
                returnSecureToken = true
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .WithHeader("X-Firebase-Locale", CultureInfo.CurrentUICulture)
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<UpdateAccountResponse>();
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(string displayName, string photoUrl,
            bool deleteDisplayName = false, bool deletePhoto = false)
        {
            List<string> deleteAttribute = new List<string>(2);
            if (deleteDisplayName) deleteAttribute.Add("DISPLAY_NAME");
            if (deletePhoto) deleteAttribute.Add("PHOTO_URL");

            var payload = new
            {
                idToken = Token,
                displayName = displayName,
                photoUrl = photoUrl,
                deleteAttribute = deleteAttribute,
                returnSecureToken = true
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .WithHeader("X-Firebase-Locale", CultureInfo.CurrentUICulture)
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<UpdateProfileResponse>();
        }

        public async Task<List<User>> GetUserDataAsync()
        {
            var payload = new
            {
                idToken = Token
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:lookup")
                .SendJsonAsync(HttpMethod.Post, payload);
            return (await response.GetJsonAsync()).users;
        }

        public async Task<UpdateProfileResponse> LinkWithEmailAndPasswordAsync(string email, string password)
        {
            var payload = new
            {
                idToken = Token,
                email = email,
                password = password,
                returnSecureToken = true
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<UpdateProfileResponse>();
        }

        public async Task<OAuthUserSignInResponse> LinkWithOAuthCredentialAsync(string oauthData, bool returnIdpCredential = false)
        {
            var payload = new
            {
                idToken = Token,
                // The URI to which the IDP redirects the user back.
                requestUri = "fluent-store://firebase_auth",
                postBody = oauthData,
                returnSecureToken = true,
                returnIdpCredential = returnIdpCredential
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:signInWithIdp")
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<OAuthUserSignInResponse>();
        }

        public async Task<UpdateProfileResponse> UnlinkProviderAsync(params string[] providerIds)
        {
            var payload = new
            {
                idToken = Token,
                deleteProvider = providerIds,
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .SendJsonAsync(HttpMethod.Post, payload);
            return await response.GetJsonAsync<UpdateProfileResponse>();
        }

        public async Task<string> RequestEmailVerification()
        {
            var payload = new
            {
                idToken = Token,
                requestType = "VERIFY_EMAIL"
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:sendOobCode")
                .WithHeader("X-Firebase-Locale", CultureInfo.CurrentUICulture)
                .SendJsonAsync(HttpMethod.Post, payload);
            return (await response.GetJsonAsync()).email;
        }

        /// <summary>
        /// Confirms a user's email.
        /// </summary>
        /// <param name="code">The action code sent to user's email for email verification.</param>
        public async Task<UpdateAccountResponse> ConfirmEmailVerificationAsync(string code)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .SendJsonAsync(HttpMethod.Post, new { oobCode = code });
            return await response.GetJsonAsync<UpdateAccountResponse>();
        }

        public async Task DeleteAccountAsync()
        {
            await GetIdentityTKBase().AppendPathSegment("accounts:delete")
                .SendJsonAsync(HttpMethod.Post, new { idToken = Token });
        }
    }
}
