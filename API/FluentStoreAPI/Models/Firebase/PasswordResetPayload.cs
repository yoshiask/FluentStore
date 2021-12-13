using Newtonsoft.Json;

namespace FluentStoreAPI.Models.Firebase
{
    public class PasswordResetPayload
    {
        /// <summary>
        /// User's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Type of the email action code. Should be "PASSWORD_RESET".
        /// </summary>
        [JsonProperty("requestType")]
        public string RequestType { get; set; } = "PASSWORD_RESET";

        public static class CommonErrors
        {
            /// <summary>
            /// There is no user record corresponding to this identifier. The user may have been deleted.
            /// </summary>
            public const string EMAIL_NOT_FOUND = nameof(EMAIL_NOT_FOUND);

            /// <summary>
            /// Password sign-in is disabled for this project.
            /// </summary>
            public const string OPERATION_NOT_ALLOWED = nameof(OPERATION_NOT_ALLOWED);

            /// <summary>
            /// The action code has expired.
            /// </summary>
            public const string EXPIRED_OOB_CODE = nameof(EXPIRED_OOB_CODE);

            /// <summary>
            /// The action code is invalid. This can happen if the code is malformed
            /// expired, or has already been used.
            /// </summary>
            public const string INVALID_OOB_CODE = nameof(INVALID_OOB_CODE);

            /// <summary>
            /// The user account has been disabled by an administrator.
            /// </summary>
            public const string USER_DISABLED = nameof(USER_DISABLED);
        }
    }
}
