using System.ComponentModel.DataAnnotations;

namespace Winstall.Models.Manifest;

/// <summary>
/// A representation of a multi-file manifest representing an app version in the OWC. v1.1.0
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class VersionManifest
{
    /// <summary>
    /// The package unique identifier
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [StringLength(128)]
    [RegularExpression(@"^[^\.\s\\/:\*\?""<>\|\x01-\x1f]{1,32}(\.[^\.\s\\/:\*\?""<>\|\x01-\x1f]{1,32}){1,3}$")]
    public string PackageIdentifier { get; set; }

    /// <summary>
    /// The package version
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [StringLength(128)]
    [RegularExpression(@"^[^\\/:\*\?""<>\|\x01-\x1f]+$")]
    public string PackageVersion { get; set; }

    /// <summary>
    /// The default package meta-data locale
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [StringLength(20)]
    [RegularExpression(@"^([a-zA-Z]{2,3}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*$")]
    public string DefaultLocale { get; set; } = "en-US";

    /// <summary>
    /// The manifest type
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string ManifestType { get; set; } = "version";

    /// <summary>
    /// The manifest syntax version
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [RegularExpression(@"^(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){2}$")]
    public string ManifestVersion { get; set; } = "1.1.0";
}
