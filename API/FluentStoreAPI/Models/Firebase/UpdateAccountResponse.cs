using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentStoreAPI.Models.Firebase
{
    public class UpdateAccountResponse
    {
        /// <summary>
        /// The uid of the current user.
        /// </summary>
        [JsonProperty("localId")]
        public string LocalID { get; set; }

        /// <summary>
        /// User's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Hash version of the password.
        /// </summary>
        [JsonProperty("passwordHash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// List of all linked provider objects which contain "providerId" and "federatedId".
        /// </summary>
        [JsonProperty("providerUserInfo")]
        public IReadOnlyList<Provider> ProviderUserInfo { get; set; }

        /// <summary>
        /// New Firebase Auth ID token for user.
        /// </summary>
        [JsonProperty("idToken")]
        public string IDToken { get; set; }

        /// <summary>
        /// A Firebase Auth refresh token.
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The number of seconds in which the ID token expires.
        /// </summary>
        [JsonProperty("expiresIn")]
        public string ExpiresIn { get; set; }

        public static class CommonErrors
        {
            /// <summary>
            /// The email address is already in use by another account.
            /// </summary>
            public const string EMAIL_EXISTS = nameof(EMAIL_EXISTS);

            /// <summary>
            /// The user's credential is no longer valid. The user must sign in again.
            /// </summary>
            public const string INVALID_ID_TOKEN = nameof(INVALID_ID_TOKEN);

            /// <summary>
            /// The password must be 6 characters long or more.
            /// </summary>
            public const string WEAK_PASSWORD = nameof(WEAK_PASSWORD);
        }
    }
}
