using Newtonsoft.Json;

namespace FluentStoreAPI.Models.Firebase
{
    public class UserSignInResponse
    {
        [JsonProperty("kind")]
        public string Kind { get; }

        /// <summary>
        /// A Firebase Auth ID token for the newly created user.
        /// </summary>
        [JsonProperty("idToken")]
        public string IDToken { get; }

        /// <summary>
        /// The email for the newly created user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; }

        /// <summary>
        /// A Firebase Auth refresh token for the newly created user.
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; }

        /// <summary>
        /// The number of seconds in which the ID token expires.
        /// </summary>
        [JsonProperty("expiresIn")]
        public string ExpiresIn { get; }

        /// <summary>
        /// The uid of the newly created user.
        /// </summary>
        [JsonProperty("localId")]
        public string LocalID { get; }

        /// <summary>
        /// Whether the email is for an existing account.
        /// </summary>
        [JsonProperty("isRegistered")]
        public bool IsRegistered { get; }

        public static class CommonErrors
        {
            /// <summary>
            /// We have blocked all requests from this device due to unusual activity. Try again later.
            /// </summary>
            public const string TOO_MANY_ATTEMPTS_TRY_LATER = nameof(TOO_MANY_ATTEMPTS_TRY_LATER);

            /// <summary>
            /// Password sign-in is disabled for this project.
            /// </summary>
            public const string OPERATION_NOT_ALLOWED = nameof(OPERATION_NOT_ALLOWED);

            /// <summary>
            /// The email address is already in use by another account.
            /// </summary>
            /// <remarks>Applies only to sign up requests.</remarks>
            public const string EMAIL_EXISTS = nameof(EMAIL_EXISTS);

            /// <summary>
            /// There is no user record corresponding to this identifier. The user may have been deleted.
            /// </summary>
            /// <remarks>Applies only to sign in and password reset requests.</remarks>
            public const string EMAIL_NOT_FOUND = nameof(EMAIL_NOT_FOUND);

            /// <summary>
            /// The password is invalid or the user does not have a password.
            /// </summary>
            /// <remarks>Applies only to sign in requests.</remarks>
            public const string INVALID_PASSWORD = nameof(INVALID_PASSWORD);

            /// <summary>
            /// The user account has been disabled by an administrator.
            /// </summary>
            /// <remarks>Applies only to sign in requests.</remarks>
            public const string USER_DISABLED = nameof(USER_DISABLED);
        }
    }
}
