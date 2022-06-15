using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Winstall.Models.Manifest;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class Dependencies
{
    /// <summary>
    /// List of Windows feature dependencies
    /// </summary>
    [MaxLength(16)]
    public List<string> WindowsFeatures { get; set; }

    /// <summary>
    /// List of Windows library dependencies
    /// </summary>
    [MaxLength(16)]
    public List<string> WindowsLibraries { get; set; }

    /// <summary>
    /// List of package dependencies from current source
    /// </summary>
    [MaxLength(16)]
    public List<PackageDependency> PackageDependencies { get; set; }

    /// <summary>
    /// List of external package dependencies
    /// </summary>
    [MaxLength(16)]
    public List<string> ExternalDependencies { get; set; }
}
