using FluentStore.SDK.Models;
using System;

namespace FluentStore.SDK.Helpers
{
    public static class InstallerTypes
    {
        /// <summary>
        /// Reduces the installer type to its most generic type.
        /// </summary>
        /// <returns>
        /// <see cref="InstallerType.Msix"/> for Windows App Packages,
        /// <see cref="InstallerType.Win32"/> for traditional Win32 installers,
        /// <see cref="InstallerType.Unknown"/> for everything else.
        /// </returns>
        public static InstallerType Reduce(this InstallerType type)
        {
            uint genericId = (uint)type >> 28;
            return genericId switch
            {
                ((uint)InstallerType.Msix >> 28) => InstallerType.Msix,
                ((uint)InstallerType.Win32 >> 28) => InstallerType.Win32,

                0 or _ => InstallerType.Unknown
            };
        }

        public static string GetExtensionDescription(this InstallerType type)
        {
            InstallerType typeReduced = type.Reduce();
            string extDesc;
            if (typeReduced == InstallerType.Msix)
            {
                extDesc = "Windows App " + (type.HasFlag(InstallerType.Bundle) ? "Bundle" : "Package");

                if (type.HasFlag(InstallerType.Encrypted))
                    extDesc = "Encrypted " + extDesc;
            }
            else
            {
                extDesc = type switch
                {
                    InstallerType.Msi => "Windows Installer",
                    InstallerType.Exe => "Installer",
                    InstallerType.Zip => "Compressed zip archive",
                    InstallerType.Inno => "Inno Setup installer",
                    InstallerType.Nullsoft => "NSIS installer",
                    InstallerType.Wix => "WiX installer",
                    InstallerType.Burn => "WiX Burn installer",

                    _ => "Unknown"
                };
            }

            return extDesc;
        }

        public static string GetExtension<TEnum>(this TEnum type) where TEnum : unmanaged, Enum
        {
            return "." + type.ToString().ToLower();
        }

        public static InstallerType FromExtension(string ext)
        {
            if (Enum.TryParse(ext, true, out InstallerType type))
                return type;
            return InstallerType.Unknown;
        }
    }
}
