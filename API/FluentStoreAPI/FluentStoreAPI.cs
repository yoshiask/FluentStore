using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Firebase;
using FluentStoreAPI.Models.Supabase;
using Flurl;
using Flurl.Http;
using Google.Apis.Firestore.v1;
using Google.Apis.Firestore.v1.Data;
using Newtonsoft.Json.Linq;
using Supabase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        public const string STORAGE_BASE_URL = "https://firebasestorage.googleapis.com/v0/b/fluent-store.appspot.com/o/";
        private const string KEY = "AIzaSyCoINaQk7QdzPryW0oZHppWnboRRPk26fQ";
        private const string NAME_PREFIX = "projects/fluent-store/databases/(default)/documents";

        private readonly FirestoreService _firestore;
        private readonly Client _supabase;

        private ProjectsResource.DatabasesResource.DocumentsResource Documents => _firestore.Projects.Databases.Documents;

        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

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
            var carouselResult = await _supabase.From<FeaturedHomeCarouselItem>().Get();
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
