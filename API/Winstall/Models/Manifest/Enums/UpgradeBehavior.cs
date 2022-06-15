using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

/// <summary>
/// The upgrade method
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public enum UpgradeBehavior
{

    [EnumMember(Value = @"install")]
    Install = 0,

    [EnumMember(Value = @"uninstallPrevious")]
    UninstallPrevious = 1,

}

