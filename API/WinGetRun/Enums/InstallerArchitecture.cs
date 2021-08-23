using Newtonsoft.Json;

namespace WinGetRun.Enums
{
    [JsonConverter(typeof(StringEnumCamelCaseConverter))]
    public enum InstallerArchitecture
    {
        X86 = 0,
        X64 = 1,
        Arm = 2,
        Arm64 = 3,
        Neutral = 4,
    }
}
