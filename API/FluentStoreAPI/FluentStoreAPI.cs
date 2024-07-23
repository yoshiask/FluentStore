using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Firebase;
using Flurl;
using Flurl.Http;
using Google.Apis.Firestore.v1;
using Google.Apis.Firestore.v1.Data;
using Newtonsoft.Json.Linq;
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

        private ProjectsResource.DatabasesResource.DocumentsResource Documents => _firestore.Projects.Databases.Documents;

        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public FluentStoreAPI()
        {
            var firestoreInit = new Google.Apis.Services.BaseClientService.Initializer
            {
                ApiKey = KEY,
                ApplicationName = "FluentStoreAPI",
            };
            _firestore = new(firestoreInit);
        }

        public FluentStoreAPI(string token, string refreshToken) : this()
        {
            Token = token;
            RefreshToken = refreshToken;
        }

        public async Task<HomePageFeatured> GetHomePageFeaturedAsync(CancellationToken token = default)
        {
            var document = await GetDocumentAsync(BuildName("featured", "home"), token);
            return new(document);
        }

        public async Task<HomePageFeatured> GetHomePageFeaturedAsync(Version appVersion)
        {
            HomePageFeatured document = await GetHomePageFeaturedAsync();

            for (int i = 0; i < document.Carousel.Count; i++)
            {
                string feat = document.Carousel[i];

                int idx = feat.LastIndexOf("/?");
                if (idx < 0) continue;

                string urn = feat.Substring(0, idx);
                string expression = feat.Substring(idx + 2);
                document.Carousel[i] = urn;

                bool show = true;
                QueryParamCollection queries = new(expression);
                if (queries.TryGetFirst("vMax", out object vMaxVal) && Version.TryParse(vMaxVal.ToString(), out Version vMax))
                    show &= appVersion <= vMax;
                if (queries.TryGetFirst("vMin", out object vMinVal) && Version.TryParse(vMinVal.ToString(), out Version vMin))
                    show &= appVersion >= vMin;
                if (!show)
                    document.Carousel.RemoveAt(i--);
            }

            return document;
        }

        public async Task<PluginDefaults> GetPluginDefaultsAsync(CancellationToken token = default)
        {
            var document = await GetDocumentAsync(BuildName("defaults", "plugins"), token);
            return new(document);
        }
    }
}
