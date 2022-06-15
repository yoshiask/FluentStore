using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Winstall.Enums;

[JsonConverter(typeof(EnumIgnoreCaseStringConverter<InstallerType>))]
public enum InstallerType
{
    [EnumMember(Value = "unknown")]
    Unknown = -1,


    [EnumMember(Value = "msix")]
    Msix = 0,

    [EnumMember(Value = "msi")]
    Msi = 1,

    [EnumMember(Value = "appx")]
    Appx = 2,

    [EnumMember(Value = "exe")]
    Exe = 3,

    [EnumMember(Value = "zip")]
    Zip = 4,

    [EnumMember(Value = "inno")]
    Inno = 5,

    [EnumMember(Value = "nullsoft")]
    Nullsoft = 6,

    [EnumMember(Value = "wix")]
    Wix = 7,

    [EnumMember(Value = "burn")]
    Burn = 8,

    [EnumMember(Value = "pwa")]
    Pwa = 9,
}
