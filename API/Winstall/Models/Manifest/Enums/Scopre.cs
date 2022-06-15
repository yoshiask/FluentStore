using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

/// <summary>
/// Scope indicates if the installer is per user or per machine
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public enum Scope
{

    [EnumMember(Value = @"user")]
    User = 0,

    [EnumMember(Value = @"machine")]
    Machine = 1,

}
