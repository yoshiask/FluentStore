using Newtonsoft.Json;

namespace Microsoft.Marketplace.Storefront.Contracts
{
    public static class Constants
    {
        public const string STOREFRONT_API_HOST = "https://storeedgefd.dsx.mp.microsoft.com";

        public const string CLIENT_ID = "7F27B536-CF6B-4C65-8638-A0F8CBDFCA65";
        public const string APP_VERSION = "22205.1401.0.0";

        public const string CAT_ALL_PRODUCTS = "DCatAll-Products";
        public const string CAT_APPS = "Apps";

        public const string DEFAULT_DEVICEFAMILY = "Windows.Desktop";
        public const string DEFAULT_ARCHITECTURE = "x86";

        public static readonly JsonSerializerSettings DefaultJsonSettings = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Default,
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };
    }
}
