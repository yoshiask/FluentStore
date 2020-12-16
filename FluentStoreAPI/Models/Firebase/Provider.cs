using Newtonsoft.Json;

namespace FluentStoreAPI.Models.Firebase
{
    public class Provider
    {
        /// <summary>
        /// The unique ID identifies the IdP account.
        /// </summary>
        [JsonProperty("federatedId")]
        public string FederatedID { get; }

        /// <summary>
        /// The linked provider ID (e.g. "google.com" for the Google provider).
        /// </summary>
        [JsonProperty("providerId")]
        public string ProviderID { get; }

        [JsonProperty("displayName")]
        public string DisplayName { get; }

        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; }
    }
}
