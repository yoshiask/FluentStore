using Newtonsoft.Json;

namespace WinGetRun.Enums
{
    [JsonConverter(typeof(EnumIgnoreCaseStringConverter<InstallerType>))]
    public enum InstallerType
    {
        Msix = 0,
        Msi = 1,
        Appx = 2,
        Exe = 3,
        Zip = 4,
        Inno = 5,
        Nullsoft = 6,
        Wix = 7,
        Burn = 8,
        Pwa = 9,
    }
}
