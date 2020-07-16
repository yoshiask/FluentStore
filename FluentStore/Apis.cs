using Refit;
using MicrosoftStore;

namespace FluentStore
{
    public static class Apis
    {
        public static IMSStoreApi MicrosoftStoreApi = RestService.For<IMSStoreApi>(
            MicrosoftStore.Constants.API_HOST,
            new RefitSettings {
                ContentSerializer = new XmlContentSerializer()
            }
        );
        public static IStorefrontApi StorefrontApi = RestService.For<IStorefrontApi>(
            MicrosoftStore.Constants.STOREFRONT_API_HOST
        );
    }
}
