using FluentStoreAPI.Models.Firebase;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        public const string IDENTITY_TK_BASE_URL = "https://identitytoolkit.googleapis.com/v1";

        private IFlurlRequest GetIdentityTKBase()
        {
            return IDENTITY_TK_BASE_URL.SetQueryParam("key", KEY).WithTimeout(10);
        }

        public async Task<UserSignInResponse> SignUpAsync(string email, string password)
        {
            var request = GetIdentityTKBase().AppendPathSegment("accounts:signUp");
            var response = await ConvertToResult<UserSignInResponse>(await request.PostJsonAsync(new { email, password, returnSecureToken = true }));
            Token = response.IDToken;
            RefreshToken = response.RefreshToken;
            return response;
        }

        public async Task<UserSignInResponse> SignInAsync(string email, string password)
        {
            var request = GetIdentityTKBase().AppendPathSegment("accounts:signInWithPassword");
            var response = await ConvertToResult<UserSignInResponse>(await request.PostJsonAsync(new { email, password, returnSecureToken = true }));
            Token = response.IDToken;
            RefreshToken = response.RefreshToken;
            return response;
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
                returnIdpCredential
            };
            var request = GetIdentityTKBase().AppendPathSegment("accounts:signInWithIdp");
            var response = await ConvertToResult<OAuthUserSignInResponse>(await request.PostJsonAsync(payload));
            Token = response.IDToken;
            RefreshToken = response.RefreshToken;
            return response;
        }

        /// <summary>
        /// Exchanges the <see cref="RefreshToken"/> to get new tokens.
        /// </summary>
        /// <remarks>Note that this does *not* update <see cref="Token"/> or <see cref="RefreshToken"/></remarks>
        public async Task<UseRefreshTokenResponse> UseRefreshToken()
        {
            var response = await "https://securetoken.googleapis.com/v1/token".SetQueryParam("key", KEY)
                .PostUrlEncodedAsync(new { grant_type = "refresh_token", refresh_token = RefreshToken });
            return await ConvertToResult<UseRefreshTokenResponse>(response);
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
                .PostJsonAsync(payload);
            return await ConvertToResult<ProvidersResponse>(response);
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
                .PostJsonAsync(payload);
            return (await response.GetJsonAsync()).email;
        }

        public async Task<PasswordResetPayload> VerifyPasswordResetAsync(string code)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:resetPassword")
                .PostJsonAsync(new { oobCode = code });
            return await ConvertToResult<PasswordResetPayload>(response);
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="code">The email action code sent to the user's email for resetting the password.</param>
        /// <param name="newPassword">The user's new password.</param>
        public async Task<PasswordResetPayload> ConfirmPasswordResetAsync(string code, string newPassword)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:resetPassword")
                .PostJsonAsync(new { oobCode = code, newPassword });
            return await ConvertToResult<PasswordResetPayload>(response);
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
                .PostJsonAsync(payload);
            return await ConvertToResult<UpdateAccountResponse>(response);
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
                .PostJsonAsync(payload);
            return await ConvertToResult<UpdateAccountResponse>(response);
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
                displayName,
                photoUrl,
                deleteAttribute,
                returnSecureToken = true
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .WithHeader("X-Firebase-Locale", CultureInfo.CurrentUICulture)
                .PostJsonAsync(payload);
            return await ConvertToResult<UpdateProfileResponse>(response);
        }

        public async Task<IReadOnlyList<User>> GetCurrentUserDataAsync()
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:lookup")
                .PostJsonAsync(new { idToken = Token });
            return (await response.GetJsonAsync<UserDataResponse>()).Users;
        }

        public async Task<UpdateProfileResponse> LinkWithEmailAndPasswordAsync(string email, string password)
        {
            var payload = new
            {
                idToken = Token,
                email,
                password,
                returnSecureToken = true
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .PostJsonAsync(payload);
            return await ConvertToResult<UpdateProfileResponse>(response);
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
                returnIdpCredential
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:signInWithIdp")
                .PostJsonAsync(payload);
            return await ConvertToResult<OAuthUserSignInResponse>(response);
        }

        public async Task<UpdateProfileResponse> UnlinkProviderAsync(params string[] providerIds)
        {
            var payload = new
            {
                idToken = Token,
                deleteProvider = providerIds,
            };
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .PostJsonAsync(payload);
            return await ConvertToResult<UpdateProfileResponse>(response);
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
                .PostJsonAsync(payload);
            return (await response.GetJsonAsync()).email;
        }

        /// <summary>
        /// Confirms a user's email.
        /// </summary>
        /// <param name="code">The action code sent to user's email for email verification.</param>
        public async Task<UpdateAccountResponse> ConfirmEmailVerificationAsync(string code)
        {
            var response = await GetIdentityTKBase().AppendPathSegment("accounts:update")
                .PostJsonAsync(new { oobCode = code });
            return await ConvertToResult<UpdateAccountResponse>(response);
        }

        public async Task DeleteAccountAsync()
        {
            await GetIdentityTKBase().AppendPathSegment("accounts:delete")
                .PostJsonAsync(new { idToken = Token });
        }
    }
}
