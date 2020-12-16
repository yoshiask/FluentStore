using Newtonsoft.Json;

namespace FluentStoreAPI.Models.Firebase
{
    public class UseRefreshTokenResponse
    {
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("id_token")]
        public string IDToken { get; set; }

        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("project_id")]
        public string ProjectID { get; set; }

        public static class CommonErrors
        {
            /// <summary>
            /// The user's credential is no longer valid. The user must sign in again.
            /// </summary>
            public const string TOKEN_EXPIRED = nameof(TOKEN_EXPIRED);

            /// <summary>
            /// The user account has been disabled by an administrator.
            /// </summary>
            public const string USER_DISABLED = nameof(USER_DISABLED);

            /// <summary>
            /// The user corresponding to the refresh token was not found. It is likely the user was deleted.
            /// </summary>
            public const string USER_NOT_FOUND = nameof(USER_NOT_FOUND);

            /// <summary>
            /// An invalid refresh token is provided.
            /// </summary>
            public const string INVALID_REFRESH_TOKEN = nameof(INVALID_REFRESH_TOKEN);

            /// <summary>
            /// The grant type specified is invalid.
            /// </summary>
            public const string INVALID_GRANT_TYPE = nameof(INVALID_GRANT_TYPE);

            /// <summary>
            /// No refresh token provided.
            /// </summary>
            public const string MISSING_REFRESH_TOKEN = nameof(MISSING_REFRESH_TOKEN);
        }
    }
}
