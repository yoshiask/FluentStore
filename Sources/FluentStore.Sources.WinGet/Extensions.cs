using FluentStore.SDK.Models;

using WinstallerArchitecture = Winstall.Models.Manifest.Enums.InstallerArchitecture;
using WinstallerType = Winstall.Enums.InstallerType;

namespace FluentStore.Sources.WinGet
{
    public static class Extensions
    {
        public static Architecture ToSDKArch(this WinstallerArchitecture wgArch)
        {
            return wgArch switch
            {
                WinstallerArchitecture.Neutral => Architecture.Neutral,
                WinstallerArchitecture.X86 => Architecture.x86,
                WinstallerArchitecture.X64 => Architecture.x64,
                WinstallerArchitecture.Arm => Architecture.Arm32,
                WinstallerArchitecture.Arm64 => Architecture.Arm64,

                _ => Architecture.Unknown,
            };
        }

        public static InstallerType ToSDKInstallerType(this WinstallerType type)
        {
            return type switch
            {
                WinstallerType.Msix => InstallerType.Msix,
                WinstallerType.Msi => InstallerType.Msi,
                WinstallerType.Appx => InstallerType.AppX,
                WinstallerType.Exe => InstallerType.Exe,
                WinstallerType.Zip => InstallerType.Zip,
                WinstallerType.Inno => InstallerType.Inno,
                WinstallerType.Nullsoft => InstallerType.Nullsoft,
                WinstallerType.Wix => InstallerType.Wix,
                WinstallerType.Burn => InstallerType.Burn,

                // we don't do that here
                WinstallerType.Pwa => InstallerType.Unknown,
                _ => InstallerType.Unknown,
            };
        }
    }
}
