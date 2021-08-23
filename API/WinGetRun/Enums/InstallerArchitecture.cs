using Newtonsoft.Json;

namespace WinGetRun.Enums
{
    [JsonConverter(typeof(EnumIgnoreCaseStringConverter<InstallerArchitecture>))]
    public enum InstallerArchitecture
    {
        X86 = 0,
        X64 = 1,
        Arm = 2,
        Arm64 = 3,
        Neutral = 4,
    }
}
