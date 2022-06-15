using System.ComponentModel.DataAnnotations;

namespace Winstall.Models.Manifest;

[System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
public class InstallerSwitches
{
    /// <summary>
    /// Silent is the value that should be passed to the installer when user chooses a silent or quiet install
    /// </summary>
    [StringLength(512, MinimumLength = 1)]
    public string Silent { get; set; }

    /// <summary>
    /// SilentWithProgress is the value that should be passed to the installer when user chooses a non-interactive install
    /// </summary>
    [StringLength(512, MinimumLength = 1)]
    public string SilentWithProgress { get; set; }

    /// <summary>
    /// Interactive is the value that should be passed to the installer when user chooses an interactive install
    /// </summary>
    [StringLength(512, MinimumLength = 1)]
    public string Interactive { get; set; }

    /// <summary>
    /// InstallLocation is the value passed to the installer for custom install location. &lt;INSTALLPATH&gt; token can be included in the switch value so that winget will replace the token with user provided path
    /// </summary>
    [StringLength(512, MinimumLength = 1)]
    public string InstallLocation { get; set; }

    /// <summary>
    /// Log is the value passed to the installer for custom log file path. &lt;LOGPATH&gt; token can be included in the switch value so that winget will replace the token with user provided path
    /// </summary>
    [StringLength(512, MinimumLength = 1)]
    public string Log { get; set; }

    /// <summary>
    /// Upgrade is the value that should be passed to the installer when user chooses an upgrade
    /// </summary>
    [StringLength(512, MinimumLength = 1)]
    public string Upgrade { get; set; }

    /// <summary>
    /// Custom switches will be passed directly to the installer by winget
    /// </summary>
    [StringLength(2048, MinimumLength = 1)]
    public string Custom { get; set; }
}
