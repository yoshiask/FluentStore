using FluentStore.SDK.Models;

namespace FluentStore.Sources.WinGet
{
    public static class Extensions
    {
        public static Architecture ToSDKArch(this WinGetRun.Enums.InstallerArchitecture wgArch)
        {
            return wgArch switch
            {
                WinGetRun.Enums.InstallerArchitecture.Neutral => Architecture.Neutral,
                WinGetRun.Enums.InstallerArchitecture.X86 => Architecture.x86,
                WinGetRun.Enums.InstallerArchitecture.X64 => Architecture.x64,
                WinGetRun.Enums.InstallerArchitecture.Arm => Architecture.Arm32,
                WinGetRun.Enums.InstallerArchitecture.Arm64 => Architecture.Arm64,

                _ => Architecture.Unknown,
            };
        }

        public static InstallerType ToSDKInstallerType(this WinGetRun.Enums.InstallerType type)
        {
            return type switch
            {
                WinGetRun.Enums.InstallerType.Msix => InstallerType.Msix,
                WinGetRun.Enums.InstallerType.Msi => InstallerType.Msi,
                WinGetRun.Enums.InstallerType.Appx => InstallerType.AppX,
                WinGetRun.Enums.InstallerType.Exe => InstallerType.Exe,
                WinGetRun.Enums.InstallerType.Zip => InstallerType.Zip,
                WinGetRun.Enums.InstallerType.Inno => InstallerType.Inno,
                WinGetRun.Enums.InstallerType.Nullsoft => InstallerType.Nullsoft,
                WinGetRun.Enums.InstallerType.Wix => InstallerType.Wix,
                WinGetRun.Enums.InstallerType.Burn => InstallerType.Burn,

                // we don't do that here
                WinGetRun.Enums.InstallerType.Pwa => InstallerType.Unknown,
                _ => InstallerType.Unknown,
            };
        }
    }
}
