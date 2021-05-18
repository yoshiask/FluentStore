using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentStoreAPI.Models
{
    public class HomePageFeatured
    {
        [JsonProperty]
        public List<string> Carousel { get; internal set; }
    }
}
