using System;

namespace FluentStore.SDK.Models
{
    [Flags]
    public enum InstallerType
    {
        Unknown = 0b0000,
        AppX = 1 << 1,
        Msix = 1 << 2,
        Bundle = 1 << 0,
        Encrypted = 1 << 3,

        Win32 = 1 << 4,

        AppXBundle = AppX | Bundle,
        MsixBundle = Msix | Bundle,
        EAppXBundle = AppX | Bundle | Encrypted,
        EMsixBundle = Msix | Bundle | Encrypted,

        Msi = Win32 + 1,
        Exe = Win32 + 2,
        Zip = Win32 + 3,    // Is this accurate?
        Inno = Win32 + 4,
        Nullsoft = Win32 + 5,
        Wix = Win32 + 6,
        Burn = Win32 + 7,
    }
}
