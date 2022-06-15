using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

/// <summary>
/// The installer's elevation requirement
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public enum ElevationRequirement
{

    [EnumMember(Value = @"elevationRequired")]
    ElevationRequired = 0,

    [EnumMember(Value = @"elevationProhibited")]
    ElevationProhibited = 1,

    [EnumMember(Value = @"elevatesSelf")]
    ElevatesSelf = 2,

}
