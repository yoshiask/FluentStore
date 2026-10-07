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
            if (typeReduced is InstallerType.Msix)
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

        public static string GetExtension(this InstallerType type)
        {
            InstallerType typeReduced = type.Reduce();
            string ext;
            if (type.HasFlag(InstallerType.AppInstaller))
            {
                ext = "appinstaller";
            }
            else if (typeReduced is InstallerType.Msix)
            {
                ext = type.HasFlag(InstallerType.AppX) ? "appx" : "msix";

                if (type.HasFlag(InstallerType.Bundle))
                    ext += "bundle";
                
                if (type.HasFlag(InstallerType.Encrypted))
                    ext = $"e{ext}";
            }
            else
            {
                ext = type switch
                {
                    InstallerType.Msi => "msi",
                    _ when typeReduced is InstallerType.Win32 => "exe",

                    _ => type.ToString().ToLowerInvariant()
                };
            }

            return $".{ext}";
        }

        public static InstallerType FromExtension(string ext)
        {
            ext = ext.TrimStart('.');

            if (Enum.TryParse(ext, true, out InstallerType type))
                return type;

            return InstallerType.Unknown;
        }
    }
}
