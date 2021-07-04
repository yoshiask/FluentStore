using Newtonsoft.Json;

namespace MicrosoftStore.Enums
{
    [JsonConverter(typeof(PlatWindowsStringConverter))]
    public enum PlatWindows
    {
        Desktop,
        Mobile,
        Team,
        Xbox,
        Holographic,
        IoT
    }
}