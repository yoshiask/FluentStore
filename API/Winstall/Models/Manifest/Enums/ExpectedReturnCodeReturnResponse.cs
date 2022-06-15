using System.Runtime.Serialization;

namespace Winstall.Models.Manifest.Enums;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public enum ExpectedReturnCodeReturnResponse
{

    [EnumMember(Value = @"packageInUse")]
    PackageInUse = 0,

    [EnumMember(Value = @"installInProgress")]
    InstallInProgress = 1,

    [EnumMember(Value = @"fileInUse")]
    FileInUse = 2,

    [EnumMember(Value = @"missingDependency")]
    MissingDependency = 3,

    [EnumMember(Value = @"diskFull")]
    DiskFull = 4,

    [EnumMember(Value = @"insufficientMemory")]
    InsufficientMemory = 5,

    [EnumMember(Value = @"noNetwork")]
    NoNetwork = 6,

    [EnumMember(Value = @"contactSupport")]
    ContactSupport = 7,

    [EnumMember(Value = @"rebootRequiredToFinish")]
    RebootRequiredToFinish = 8,

    [EnumMember(Value = @"rebootRequiredForInstall")]
    RebootRequiredForInstall = 9,

    [EnumMember(Value = @"rebootInitiated")]
    RebootInitiated = 10,

    [EnumMember(Value = @"cancelledByUser")]
    CancelledByUser = 11,

    [EnumMember(Value = @"alreadyInstalled")]
    AlreadyInstalled = 12,

    [EnumMember(Value = @"downgrade")]
    Downgrade = 13,

    [EnumMember(Value = @"blockedByPolicy")]
    BlockedByPolicy = 14,

}
