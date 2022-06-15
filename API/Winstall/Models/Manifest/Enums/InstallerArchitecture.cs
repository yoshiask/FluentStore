using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public enum InstallerArchitecture
{
    [EnumMember(Value = "x86")]
    X86 = 0,

    [EnumMember(Value = "x64")]
    X64 = 1,

    [EnumMember(Value = "arm")]
    Arm = 2,

    [EnumMember(Value = "arm64")]
    Arm64 = 3,

    [EnumMember(Value = "neutral")]
    Neutral = 4,
}
