namespace FluentStore.SDK.Models
{
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
        AppInstaller = Msix | 1 << 2,

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

    public enum WindowsPlatform : uint
    {
        Unknown     = 0b0000,
        Windows8x   = 0b0010,
        Universal   = 0b1000,

        Desktop = (1 << 5),
        Mobile = (1 << 6),
        Team = (1 << 7),
        Xbox = (1 << 8),
        Holographic = (1 << 10),
        IoT = (1 << 11)
    }

    public static class SharedResources
    {
        public const string SuccessColor = "rsrc.SuccessColor";
        public const string InfoColor = "rsrc.InfoColor";
        public const string WarningColor = "rsrc.WarningColor";
        public const string ErrorColor = "rsrc.ErrorColor";
        public const string SymbolFont = "rsrc.SymbolThemeFontFamily";

        public static bool TryGetName(string value, out string name)
        {
            name = null;

            if (value is null)
                return false;

            if (value.StartsWith("rsrc."))
            {
                name = value[5..];
                return true;
            }

            return false;
        }
    }
}
