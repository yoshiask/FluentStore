using System.ComponentModel.DataAnnotations;

namespace Winstall.Models.Manifest;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public partial class PackageDependency
{
    [Required(AllowEmptyStrings = true)]
    [StringLength(128)]
    [RegularExpression(@"^[^\.\s\\/:\*\?""<>\|\x01-\x1f]{1,32}(\.[^\.\s\\/:\*\?""<>\|\x01-\x1f]{1,32}){1,3}$")]
    public string PackageIdentifier { get; set; }

    [StringLength(128)]
    [RegularExpression(@"^[^\\/:\*\?""<>\|\x01-\x1f]+$")]
    public string MinimumVersion { get; set; }
}
