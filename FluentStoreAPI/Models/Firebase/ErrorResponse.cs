using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentStoreAPI.Models.Firebase
{
    public class ErrorResponse
    {
        [JsonProperty("code")]
        public int Code { get; }

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("errors")]
        public IReadOnlyList<Error> Errors { get; }
    }

    public class Error
    {
        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("domain")]
        public string Domain { get; }

        [JsonProperty("reason")]
        public string Reason { get; }
    }
}
