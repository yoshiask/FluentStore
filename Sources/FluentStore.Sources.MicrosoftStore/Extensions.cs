using FluentStore.SDK.Models;

using WGetArchitecture = WinGet.Sharp.Enums.InstallerArchitecture;
using WGetType = WinGet.Sharp.Enums.InstallerType;

namespace FluentStore.Sources.MicrosoftStore;

public static class Extensions
{
    public static Architecture ToSDKArch(this WGetArchitecture wgArch)
    {
        return wgArch switch
        {
            WGetArchitecture.Neutral => Architecture.Neutral,
            WGetArchitecture.X86 => Architecture.x86,
            WGetArchitecture.X64 => Architecture.x64,
            WGetArchitecture.Arm => Architecture.Arm32,
            WGetArchitecture.Arm64 => Architecture.Arm64,

            _ => Architecture.Unknown,
        };
    }

    public static InstallerType ToSDKInstallerType(this WGetType type)
    {
        return type switch
        {
            WGetType.Msix => InstallerType.Msix,
            WGetType.Msi => InstallerType.Msi,
            WGetType.Appx => InstallerType.AppX,
            WGetType.Exe => InstallerType.Exe,
            WGetType.Zip => InstallerType.Zip,
            WGetType.Inno => InstallerType.Inno,
            WGetType.Nullsoft => InstallerType.Nullsoft,
            WGetType.Wix => InstallerType.Wix,
            WGetType.Burn => InstallerType.Burn,

            // we don't do that here
            WGetType.Pwa => InstallerType.Unknown,
            _ => InstallerType.Unknown,
        };
    }
}
