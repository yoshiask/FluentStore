using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Supabase;
using Supabase;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        private readonly Client _supabase;

        public FluentStoreAPI() : this(Constants.SUPABASE_URL, Constants.SUPABASE_KEY)
        {
        }

        public FluentStoreAPI(string supabaseUrl, string supabaseKey)
        {
            SupabaseOptions options = new()
            {
                AutoRefreshToken = true,
            };
            
            _supabase = new(supabaseUrl, supabaseKey, options);
        }

        public async Task InitAsync() => await _supabase.InitializeAsync();

        public async Task<HomePageFeatured> GetHomePageFeaturedAsync(CancellationToken token = default)
        {
            var carouselResult = await _supabase.From<FeaturedHomeCarouselItem>().Get(token);
            var carouselItems = carouselResult.Models;

            return new()
            {
                Carousel = carouselItems,
            };
        }

        public async Task<HomePageFeatured> GetHomePageFeaturedAsync(Version appVersion, CancellationToken token = default)
        {
            HomePageFeatured document = await GetHomePageFeaturedAsync(token);

            for (int i = 0; i < document.Carousel.Count; i++)
            {
                var feat = document.Carousel[i];

                bool show = appVersion >= feat.MinVersion
                    && appVersion <= feat.MaxVersion;

                if (!show)
                    document.Carousel.RemoveAt(i--);
            }

            return document;
        }
    }
}
