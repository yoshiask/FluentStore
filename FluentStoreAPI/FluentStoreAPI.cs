using FluentStoreAPI.Models;
using FluentStoreAPI.Models.Firebase;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        public const string STORAGE_BASE_URL = "https://firebasestorage.googleapis.com/v0/b/fluent-store.appspot.com/o/";
        public const string FIRESTORE_BASE_URL = "https://firestore.googleapis.com/v1/projects/fluent-store/databases/(default)/documents/";
        private const string KEY = "AIzaSyCoINaQk7QdzPryW0oZHppWnboRRPk26fQ";

        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public FluentStoreAPI() { }
        public FluentStoreAPI(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }

        static readonly Newtonsoft.Json.JsonSerializerSettings _json = new Newtonsoft.Json.JsonSerializerSettings()
        {
            Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e) => 
            {
                if (e.CurrentObject is null)
                {
                    string log = $"Serialization on path {e.ErrorContext.Path} generated an exception {e.ErrorContext.Error.GetType().Name} with warning {e.ErrorContext.Error.Message}";
#if DEBUG
                    System.Diagnostics.Debugger.Break();
                    System.Diagnostics.Debug.WriteLine(log);
#else
                    Console.WriteLine(log);
#endif
                }
                else
                {
                    string log = $"Serialization on {e.CurrentObject.GetType().Name} had issues with path {e.ErrorContext.Path} and generated an exception {e.ErrorContext.Error.GetType().Name} with warning {e.ErrorContext.Error.Message}";
#if DEBUG
                    System.Diagnostics.Debugger.Break();
                    System.Diagnostics.Debug.WriteLine(log);
#else
                    Console.WriteLine(log);
#endif
                }

                e.ErrorContext.Handled = true;
            },
            DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
            MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
            ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,
        };
        private async Task<TResult> ConvertToResult<TResult>(IFlurlResponse response)
        {
            var json = await response.GetStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(json);
        }

        public async Task<HomePageFeatured> GetHomePageFeaturedAsync()
        {
            var document = await GetDocument(false, "home", "featured");
            return document.Transform<HomePageFeatured>();
        }
    }
}
