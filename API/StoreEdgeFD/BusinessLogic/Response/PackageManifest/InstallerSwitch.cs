using System.ComponentModel.DataAnnotations;
using WinGet.Sharp.Models;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest;

public class InstallerSwitch
{
    [StringLength(512, MinimumLength = 1)]
    public string Silent { get; set; }

    [StringLength(512, MinimumLength = 1)]
    public string SilentWithProgress { get; set; }

    [StringLength(512, MinimumLength = 1)]
    public string Interactive { get; set; }

    [StringLength(512, MinimumLength = 1)]
    public string InstallLocation { get; set; }

    [StringLength(512, MinimumLength = 1)]
    public string Log { get; set; }

    [StringLength(512, MinimumLength = 1)]
    public string Upgrade { get; set; }

    [StringLength(2048, MinimumLength = 1)]
    public string Custom { get; set; }

    public InstallerSwitches ToWinGet()
    {
        return new()
        {
            Silent = Silent,
            SilentWithProgress = SilentWithProgress,
            Interactive = Interactive,
            InstallLocation = InstallLocation,
            Log = Log,
            Upgrade = Upgrade,
            Custom = Custom
        };
    }
}
