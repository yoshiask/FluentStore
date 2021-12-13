using System;

namespace FluentStore.SDK.Models
{
    [Flags]
    public enum InstallerType : uint
    {
        Unknown = 0,

        // Modern packages
        Msix = 1 << 28,

        Bundle = 1 << 0,
        Encrypted = 1 << 1,
        AppX = Msix | 1 << 3,
        AppXBundle = AppX | Bundle,
        MsixBundle = Msix | Bundle,
        EAppXBundle = AppX | Bundle | Encrypted,
        EMsixBundle = Msix | Bundle | Encrypted,

        // Traditional [Win32] installers
        Win32 = 2 << 28,

        Msi = Win32 + 1,
        Exe = Win32 + 2,
        Zip = Win32 + 3,    // Is this accurate?
        Inno = Win32 + 4,
        Nullsoft = Win32 + 5,
        Wix = Win32 + 6,
        Burn = Win32 + 7,
    }
}
