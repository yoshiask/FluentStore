using Newtonsoft.Json;

namespace Microsoft.Marketplace.Storefront.Contracts.Enums
{
    [JsonConverter(typeof(PlatWindowsStringConverter))]
    public enum PlatWindows
    {
        Desktop,
        Mobile,
        Team,
        Xbox,
        Holographic,
        IoT,

        Unknown = -1
    }
}