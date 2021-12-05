using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class HomePageFeatured
    {
        [JsonProperty]
        public List<string> Carousel { get; internal set; }

        /// <summary>
        /// Used by <see cref="Document"/> for deserialization
        /// </summary>
        public void SetCarousel(List<object> objItems)
        {
            Carousel = objItems.Cast<string>().ToList();
        }
    }
}
