using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.4.3.0 (Newtonsoft.Json v11.0.0.0)")]
public enum IconFileType
{
    [EnumMember(Value = @"png")]
    Png = 0,

    [EnumMember(Value = @"jpeg")]
    Jpeg = 1,

    [EnumMember(Value = @"ico")]
    Ico = 2,

}

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.4.3.0 (Newtonsoft.Json v11.0.0.0)")]
public enum IconResolution
{
    [EnumMember(Value = @"custom")]
    Custom = 0,

    [EnumMember(Value = @"16x16")]
    _16x16 = 1,

    [EnumMember(Value = @"20x20")]
    _20x20 = 2,

    [EnumMember(Value = @"24x24")]
    _24x24 = 3,

    [EnumMember(Value = @"30x30")]
    _30x30 = 4,

    [EnumMember(Value = @"32x32")]
    _32x32 = 5,

    [EnumMember(Value = @"36x36")]
    _36x36 = 6,

    [EnumMember(Value = @"40x40")]
    _40x40 = 7,

    [EnumMember(Value = @"48x48")]
    _48x48 = 8,

    [EnumMember(Value = @"60x60")]
    _60x60 = 9,

    [EnumMember(Value = @"64x64")]
    _64x64 = 10,

    [EnumMember(Value = @"72x72")]
    _72x72 = 11,

    [EnumMember(Value = @"80x80")]
    _80x80 = 12,

    [EnumMember(Value = @"96x96")]
    _96x96 = 13,

    [EnumMember(Value = @"256x256")]
    _256x256 = 14,

}

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.4.3.0 (Newtonsoft.Json v11.0.0.0)")]
public enum IconTheme
{
    [EnumMember(Value = @"default")]
    Default = 0,

    [EnumMember(Value = @"light")]
    Light = 1,

    [EnumMember(Value = @"dark")]
    Dark = 2,

    [EnumMember(Value = @"highContrast")]
    HighContrast = 3,

}
