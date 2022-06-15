using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public enum InstallModes
{

    [EnumMember(Value = @"interactive")]
    Interactive = 0,

    [EnumMember(Value = @"silent")]
    Silent = 1,

    [EnumMember(Value = @"silentWithProgress")]
    SilentWithProgress = 2,

}
