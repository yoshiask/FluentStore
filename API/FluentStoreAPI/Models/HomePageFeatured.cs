using System.Collections.Generic;

namespace FluentStoreAPI.Models
{
    public class HomePageFeatured
    {
        public required List<FeaturedHomeCarouselItem> Carousel { get; init; }
    }
}
