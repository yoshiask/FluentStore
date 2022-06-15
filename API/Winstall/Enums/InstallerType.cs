using Newtonsoft.Json;

namespace Winstall.Enums;

[JsonConverter(typeof(EnumIgnoreCaseStringConverter<InstallerType>))]
public enum InstallerType
{
    Unknown     = -1,

    Msix        = 0,
    Msi         = 1,
    Appx        = 2,
    Exe         = 3,
    Zip         = 4,
    Inno        = 5,
    Nullsoft    = 6,
    Wix         = 7,
    Burn        = 8,
    Pwa         = 9,
}
