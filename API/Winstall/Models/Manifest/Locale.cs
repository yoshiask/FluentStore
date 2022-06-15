using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Winstall.Models.Manifest;

/// <summary>
/// A representation of a multiple-file manifest representing app metadata in other locale in the OWC. v1.1.0
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class Locale
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
    /// The package meta-data locale
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [StringLength(20)]
    [RegularExpression(@"^([a-zA-Z]{2,3}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*$")]
    public string PackageLocale { get; set; }

    /// <summary>
    /// The publisher name
    /// </summary>
    [StringLength(256, MinimumLength = 2)]
    public string Publisher { get; set; }

    /// <summary>
    /// The publisher home page
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string PublisherUrl { get; set; }

    /// <summary>
    /// The publisher support page
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string PublisherSupportUrl { get; set; }

    /// <summary>
    /// The publisher privacy page or the package privacy page
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string PrivacyUrl { get; set; }

    /// <summary>
    /// The package author
    /// </summary>
    [StringLength(256, MinimumLength = 2)]
    public string Author { get; set; }

    /// <summary>
    /// The package name
    /// </summary>
    [StringLength(256, MinimumLength = 2)]
    public string PackageName { get; set; }

    /// <summary>
    /// The package home page
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string PackageUrl { get; set; }

    /// <summary>
    /// The package license
    /// </summary>
    [StringLength(512, MinimumLength = 3)]
    public string License { get; set; }

    /// <summary>
    /// The license page
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string LicenseUrl { get; set; }

    /// <summary>
    /// The package copyright
    /// </summary>
    [StringLength(512, MinimumLength = 3)]
    public string Copyright { get; set; }

    /// <summary>
    /// The package copyright page
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string CopyrightUrl { get; set; }

    /// <summary>
    /// The short package description
    /// </summary>
    [StringLength(256, MinimumLength = 3)]
    public string ShortDescription { get; set; }

    /// <summary>
    /// The full package description
    /// </summary>
    [StringLength(10000, MinimumLength = 3)]
    public string Description { get; set; }

    /// <summary>
    /// List of additional package search terms
    /// </summary>
    [MaxLength(16)]
    public List<string> Tags { get; set; }

    [MaxLength(128)]
    public List<Agreement> Agreements { get; set; }

    /// <summary>
    /// The package release notes
    /// </summary>
    [StringLength(10000, MinimumLength = 1)]
    public string ReleaseNotes { get; set; }

    /// <summary>
    /// The package release notes url
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string ReleaseNotesUrl { get; set; }

    /// <summary>
    /// The manifest type
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public virtual string ManifestType { get; set; } = "locale";

    /// <summary>
    /// The manifest syntax version
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    [RegularExpression(@"^(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){2}$")]
    public string ManifestVersion { get; set; } = "1.1.0";
}
