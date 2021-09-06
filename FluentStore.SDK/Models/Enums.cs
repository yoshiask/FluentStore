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

        AppXBundle = AppX | Bundle,
        MsixBundle = Msix | Bundle,
        EAppXBundle = AppX | Bundle | Encrypted,
        EMsixBundle = Msix | Bundle | Encrypted,
    }
}
