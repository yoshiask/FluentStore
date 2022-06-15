using System.ComponentModel.DataAnnotations;
using Winstall.Enums;

namespace Winstall.Models.Manifest;

/// <summary>
/// Various key values under installer's ARP entry
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class AppsAndFeaturesEntry
{
    /// <summary>
    /// The DisplayName registry value
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    public string DisplayName { get; set; }

    /// <summary>
    /// The Publisher registry value
    /// </summary>
    [StringLength(256, MinimumLength = 1)]
    public string Publisher { get; set; }

    /// <summary>
    /// The DisplayVersion registry value
    /// </summary>
    [StringLength(128, MinimumLength = 1)]
    public string DisplayVersion { get; set; }

    public string ProductCode { get; set; }

    public string UpgradeCode { get; set; }

    public InstallerType? InstallerType { get; set; }
}
