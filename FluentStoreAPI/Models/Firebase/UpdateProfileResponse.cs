using Newtonsoft.Json;
namespace FluentStoreAPI.Models.Firebase
{
    public class UpdateProfileResponse : UpdateAccountResponse
    {
        /// <summary>
        /// User's new display name.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; }

        /// <summary>
        /// User's new photo url.
        /// </summary>
        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; }
    }
}
