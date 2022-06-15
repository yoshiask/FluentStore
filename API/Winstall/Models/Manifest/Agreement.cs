using System.ComponentModel.DataAnnotations;
using YamlDotNet.Serialization;

namespace Winstall.Models.Manifest;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class Agreement
{
    /// <summary>
    /// The label of the Agreement. i.e. EULA, AgeRating, etc. This field should be localized. Either Agreement or AgreementUrl is required. When we show the agreements, we would Bold the AgreementLabel
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string AgreementLabel { get; set; }

    /// <summary>
    /// The agreement text content.
    /// </summary>
    [YamlMember(Alias = "Agreement")]
    [StringLength(10000, MinimumLength = 1)]
    public string AgreementContent { get; set; }

    /// <summary>
    /// The agreement URL.
    /// </summary>
    [StringLength(2048)]
    [RegularExpression(@"^([Hh][Tt][Tt][Pp][Ss]?)://.+$")]
    public string AgreementUrl { get; set; }
}