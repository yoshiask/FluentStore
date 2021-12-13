using Newtonsoft.Json;

namespace FluentStoreAPI.Models.Firebase
{
    public class UserSignInResponse
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// A Firebase Auth ID token for the newly created user.
        /// </summary>
        [JsonProperty("idToken")]
        public string IDToken { get; set; }

        /// <summary>
        /// The email for the newly created user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// A Firebase Auth refresh token for the newly created user.
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The number of seconds in which the ID token expires.
        /// </summary>
        [JsonProperty("expiresIn")]
        public string ExpiresIn { get; set; }

        /// <summary>
        /// The uid of the newly created user.
        /// </summary>
        [JsonProperty("localId")]
        public string LocalID { get; set; }

        /// <summary>
        /// Whether the email is for an existing account.
        /// </summary>
        [JsonProperty("isRegistered")]
        public bool IsRegistered { get; set; }

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
            /// An email address was not provided.
            /// </summary>
            public const string MISSING_EMAIL = nameof(MISSING_EMAIL);

            /// <summary>
            /// A password was not provided.
            /// </summary>
            public const string MISSING_PASSWORD = nameof(MISSING_PASSWORD);

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

            public static string GetMessage(string error)
            {
                switch (error)
                {
                    case TOO_MANY_ATTEMPTS_TRY_LATER:
                        return "Too many attempts, try again later";
                    case OPERATION_NOT_ALLOWED:
                        return "Password sign-in has been disabled";
                    case MISSING_EMAIL:
                        return "An email address was not provided";
                    case MISSING_PASSWORD:
                        return "A password was not provided";
                    case EMAIL_EXISTS:
                        return "An account with that email already exists";
                    case EMAIL_NOT_FOUND:
                        return "An account with that email could not be found";
                    case INVALID_PASSWORD:
                        return "The password was incorrect";
                    case USER_DISABLED:
                        return "Account disabled by administrator";

                    default:
                        return "An unknown error occured:\r\n" + error;
                }
            }
        }
    }
}
