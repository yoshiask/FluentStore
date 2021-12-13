using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentStoreAPI.Models.Firebase
{
    public class UserDataResponse
    {

        [JsonProperty("users")]
        public IReadOnlyList<User> Users { get; set; }
    }
}
