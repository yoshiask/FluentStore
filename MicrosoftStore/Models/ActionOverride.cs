using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class ActionOverride
    {
        public string ActionType { get; set; }
        public List<ActionOverrideCase> Cases { get; set; }
    }

    public class ActionOverrideCase
    {
        public ActionOverrideConditions Conditions { get; set; }
        public bool Visibility { get; set; }
        [JsonProperty(PropertyName = "Uri")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri
        {
            get
            {
                try
                {
                    return new Uri(Url);
                }
                catch { return null; }
            }
        }
    }

    public class ActionOverrideConditions
    {
        public List<string> ClassicAppKeys { get; set; }
        public Platform Platform { get; set; }
    }
}
