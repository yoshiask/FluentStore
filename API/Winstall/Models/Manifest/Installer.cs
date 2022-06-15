using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Winstall.Enums;
using Winstall.Models.Manifest.Enums;

namespace Winstall.Models.Manifest;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class Installer
{
    [StringLength(20)]
    [RegularExpression(@"^([a-zA-Z]{2,3}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*$")]
    public string InstallerLocale { get; set; }

    /// <summary>
    /// The installer supported operating system
    /// </summary>
    [MaxLength(2)]
    public List<Platform> Platform { get; set; }

    [RegularExpression(@"^(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){0,3}$")]
    public string MinimumOSVersion { get; set; }

    /// <summary>
    /// The installer target architecture
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public InstallerArchitecture Architecture { get; set; }

    public InstallerType? InstallerType { get; set; }

    public Scope? Scope { get; set; }

    /// <summary>
    /// The installer Url
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string InstallerUrl { get; set; }

    /// <summary>
    /// Sha256 is required. Sha256 of the installer
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [RegularExpression(@"^[A-Fa-f0-9]{64}$")]
    public string InstallerSha256 { get; set; }

    /// <summary>
    /// SignatureSha256 is recommended for appx or msix. It is the sha256 of signature file inside appx or msix. Could be used during streaming install if applicable
    /// </summary>
    [RegularExpression(@"^[A-Fa-f0-9]{64}$")]
    public string SignatureSha256 { get; set; }

    [MaxLength(3)]
    public List<InstallModes> InstallModes { get; set; }

    public InstallerSwitches InstallerSwitches { get; set; }

    /// <summary>
    /// List of additional non-zero installer success exit codes other than known default values by winget
    /// </summary>
    [MaxLength(16)]
    public List<long> InstallerSuccessCodes { get; set; }

    /// <summary>
    /// Installer exit codes for common errors
    /// </summary>
    [MaxLength(128)]
    public List<ExpectedReturnCode> ExpectedReturnCodes { get; set; }

    public UpgradeBehavior? UpgradeBehavior { get; set; }

    /// <summary>
    /// List of commands or aliases to run the package
    /// </summary>
    [MaxLength(16)]
    public List<string> Commands { get; set; }

    /// <summary>
    /// List of protocols the package provides a handler for
    /// </summary>
    [MaxLength(16)]
    public List<string> Protocols { get; set; }

    /// <summary>
    /// List of file extensions the package could support
    /// </summary>
    [MaxLength(256)]
    public List<string> FileExtensions { get; set; }

    public Dependencies Dependencies { get; set; }

    [StringLength(255)]
    [RegularExpression(@"^[A-Za-z0-9][-\.A-Za-z0-9]+_[A-Za-z0-9]{13}$")]
    public string PackageFamilyName { get; set; }

    [StringLength(255, MinimumLength = 1)]
    public string ProductCode { get; set; }

    /// <summary>
    /// List of APPX or MSIX installer capabilities
    /// </summary>
    [MaxLength(1000)]
    public List<string> Capabilities { get; set; }

    /// <summary>
    /// List of APPX or MSIX installer restricted capabilities
    /// </summary>
    [MaxLength(1000)]
    public List<string> RestrictedCapabilities { get; set; }

    /// <summary>
    /// Optional markets the package is allowed to be installed
    /// </summary>
    public List<string> Markets { get; set; }

    /// <summary>
    /// Optional markets the package is not allowed to be installed
    /// </summary>
    public List<string> ExcludedMarkets { get; set; }

    public bool? InstallerAbortsTerminal { get; set; }

    //[Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
    public System.DateTimeOffset? ReleaseDate { get; set; }

    public bool? InstallLocationRequired { get; set; }

    public bool? RequireExplicitUpgrade { get; set; }

    /// <summary>
    /// List of OS architectures the installer does not support
    /// </summary>
    public List<InstallerArchitecture> UnsupportedOSArchitectures { get; set; }

    /// <summary>
    /// List of ARP entries.
    /// </summary>
    [MaxLength(128)]
    public List<AppsAndFeaturesEntry> AppsAndFeaturesEntries { get; set; }

    public ElevationRequirement? ElevationRequirement { get; set; }
}

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
internal class DateFormatConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
{
    public DateFormatConverter()
    {
        DateTimeFormat = "yyyy-MM-dd";
    }
}

