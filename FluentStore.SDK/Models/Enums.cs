using System;

namespace FluentStore.SDK.Models
{
    [Flags]
    public enum InstallerType
    {
        Unknown = 0b0000,
        AppX = 0b0010,
        Msix = 0b0100,
        Bundle = 0b0001,
        Encrypted = 0b1000,

        AppXBundle = AppX | Bundle,
        MsixBundle = Msix | Bundle,
        EAppXBundle = AppX | Bundle | Encrypted,
        EMsixBundle = Msix | Bundle | Encrypted,
    }
}
