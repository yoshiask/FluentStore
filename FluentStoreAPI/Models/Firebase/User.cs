using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentStoreAPI.Models.Firebase
{
    public class User
    {
        /// <summary>
        /// The uid of the current user.
        /// </summary>
        [JsonProperty("localId")]
        public string LocalID { get; }

        /// <summary>
        /// User's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; }

        /// <summary>
        /// Whether or not the account's email has been verified.
        /// </summary>
        [JsonProperty("emailVerified")]
        public bool IsEmailVerified { get; }

        /// <summary>
        /// The display name for the account.
        /// </summary>
        [JsonProperty("DisplayName")]
        public string DisplayName { get; }

        /// <summary>
        /// List of all linked provider objects which contain "providerId" and "federatedId".
        /// </summary>
        [JsonProperty("providerUserInfo")]
        public IReadOnlyList<Provider> ProviderUserInfo { get; }

        /// <summary>
        /// User's new photo url.
        /// </summary>
        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; }

        /// <summary>
        /// Hash version of the password.
        /// </summary>
        [JsonProperty("passwordHash")]
        public string PasswordHash { get; }

        /// <summary>
        /// The timestamp, in milliseconds, that the account password was last changed.
        /// </summary>
        [JsonProperty("passwordUpdatedAt")]
        public double PasswordUpdatedAt { get; }

        /// <summary>
        /// The timestamp, in seconds, which marks a boundary, before which Firebase ID token are considered revoked.
        /// </summary>
        [JsonProperty("validSince")]
        public string ValidSince { get; }

        /// <summary>
        /// Whether the account is disabled or not.
        /// </summary>
        [JsonProperty("disabled")]
        public bool IsDisabled { get; }

        /// <summary>
        /// The timestamp, in milliseconds, that the account last logged in at.
        /// </summary>
        [JsonProperty("lastLoginAt")]
        public string LastLoginAt { get; }

        /// <summary>
        /// The timestamp, in milliseconds, that the account was created at.
        /// </summary>
        [JsonProperty("createdAt")]
        public string CreatedAt { get; }

        /// <summary>
        /// Whether the account is authenticated by the developer.
        /// </summary>
        [JsonProperty("customAuth")]
        public bool IsCustomAuth { get; }

        public static class CommonErrors
        {
            /// <summary>
            /// The user's credential is no longer valid. The user must sign in again.
            /// </summary>
            public const string INVALID_ID_TOKEN = nameof(INVALID_ID_TOKEN);

            /// <summary>
            /// There is no user record corresponding to this identifier. The user may have been deleted.
            /// </summary>
            public const string USER_NOT_FOUND = nameof(USER_NOT_FOUND);
        }
    }
}
