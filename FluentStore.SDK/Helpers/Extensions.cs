using System;
using System.Collections.Generic;
using Windows.System;

namespace FluentStore.SDK.Helpers
{
    public static class Extensions
    {
        public static TOut GetOrDefault<TOut, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TOut defaultValue = default)
        {
            // If dictionary is null or key is invalid, return default.
            if (dictionary == null || key == null)
            {
                return defaultValue;
            }

            // If setting doesn't exist, create it.
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = (TValue)(object)defaultValue;
            }

            return (TOut)(object)dictionary[key];
        }

        public static ProcessorArchitecture ToWinRTArch(this WinGetRun.Enums.InstallerArchitecture wgArch)
        {
            switch (wgArch)
            {
                case WinGetRun.Enums.InstallerArchitecture.Neutral:
                    return ProcessorArchitecture.Neutral;
                case WinGetRun.Enums.InstallerArchitecture.X86:
                    return ProcessorArchitecture.X86;
                case WinGetRun.Enums.InstallerArchitecture.X64:
                    return ProcessorArchitecture.X64;
                case WinGetRun.Enums.InstallerArchitecture.Arm:
                    return ProcessorArchitecture.Arm;
                case WinGetRun.Enums.InstallerArchitecture.Arm64:
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
                        return ProcessorArchitecture.Arm64;
                    goto default;

                default:
                    return ProcessorArchitecture.Unknown;
            }
        }

        public static Models.InstallerType ToInstallerType(this WinGetRun.Enums.InstallerType type)
        {
            return type switch
            {
                WinGetRun.Enums.InstallerType.Msix => Models.InstallerType.Msix,
                WinGetRun.Enums.InstallerType.Msi => Models.InstallerType.Msi,
                WinGetRun.Enums.InstallerType.Appx => Models.InstallerType.AppX,
                WinGetRun.Enums.InstallerType.Exe => Models.InstallerType.Exe,
                WinGetRun.Enums.InstallerType.Zip => Models.InstallerType.Zip,
                WinGetRun.Enums.InstallerType.Inno => Models.InstallerType.Inno,
                WinGetRun.Enums.InstallerType.Nullsoft => Models.InstallerType.Nullsoft,
                WinGetRun.Enums.InstallerType.Wix => Models.InstallerType.Wix,
                WinGetRun.Enums.InstallerType.Burn => Models.InstallerType.Burn,

                WinGetRun.Enums.InstallerType.Pwa => Models.InstallerType.Unknown,
                _ => Models.InstallerType.Unknown,
            };
        }

        public static Models.InstallerType Reduce(this Models.InstallerType type)
        {
            if (type.HasFlag(Models.InstallerType.AppX) || type.HasFlag(Models.InstallerType.Msix))
                return Models.InstallerType.AppX;
            else if (type.HasFlag(Models.InstallerType.Win32))
                return Models.InstallerType.Win32;
            else
                return Models.InstallerType.Unknown;
        }

        public static string GetExtension<TEnum>(this TEnum type) where TEnum : unmanaged, Enum
        {
            return "." + type.ToString().ToLower();
        }

        public static bool IsAtLeast<TEnum>(this TEnum a, TEnum b) where TEnum : unmanaged, Enum
        {
            return a.AsUInt64() >= b.AsUInt64();
        }

        public static bool IsLessThan<TEnum>(this TEnum a, TEnum b) where TEnum : unmanaged, Enum
        {
            return a.AsUInt64() < b.AsUInt64();
        }

        public static unsafe ulong AsUInt64<TEnum>(this TEnum item) where TEnum : unmanaged, Enum
        {
            ulong x;
            if (sizeof(TEnum) == 1)
                x = *(byte*)(&item);
            else if (sizeof(TEnum) == 2)
                x = *(ushort*)(&item);
            else if (sizeof(TEnum) == 4)
                x = *(uint*)(&item);
            else if (sizeof(TEnum) == 8)
                x = *(ulong*)(&item);
            else
                throw new ArgumentException("Argument is not a usual enum type; it is not 1, 2, 4, or 8 bytes in length.");
            return x;
        }
    }
}
