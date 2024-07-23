using Google.Apis.Firestore.v1.Data;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class HomePageFeatured
    {
        public HomePageFeatured() { }

        internal HomePageFeatured(Document d)
        {
            Carousel = d.Fields[nameof(Carousel)].ArrayValue.Values
                .Select(v => v.StringValue).ToList();
        }

        public List<string> Carousel { get; internal set; }
    }
}
