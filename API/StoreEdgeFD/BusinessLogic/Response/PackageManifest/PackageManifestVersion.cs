using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class PackageManifestVersion
    {
        public string PackageVersion { get; set; }
        public DefaultLocale DefaultLocale { get; set; }
        public List<Locale> Locales { get; set; }
        public List<SparkInstaller> Installers { get; set; }

        public WinGetRun.Models.Manifest ToWinGetRunManifest(int idx)
            => ToWinGetRunManifest(Installers[idx]);

        public WinGetRun.Models.Manifest ToWinGetRunManifest(SparkInstaller installer)
        {
            WinGetRun.Models.Manifest manifest = new()
            {
                Version = PackageVersion,
                Publisher = DefaultLocale.Publisher,
                LicenseUrl = DefaultLocale.License,
                Tags = string.Join(",", DefaultLocale.Tags),
                Switches = installer.InstallerSwitches,
                Description = DefaultLocale.Description,
                Name = DefaultLocale.PackageName,
                InstallerType = installer.InstallerType,
            };

            manifest.Installers ??= new(Installers.Count);
            foreach (SparkInstaller spark in Installers)
            {
                if (spark == installer)
                    continue;
                manifest.Installers.Add(spark.ToWinGetRunInstaller());
            }
            manifest.Installers.Insert(0, installer.ToWinGetRunInstaller());

            return manifest;
        }
    }
}