using System;
using System.Collections.Generic;
using Windows.System;
using WinGetRun.Enums;

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

        public static ProcessorArchitecture ToWinRTArch(this InstallerArchitecture wgArch)
        {
            switch (wgArch)
            {
                case InstallerArchitecture.Neutral:
                    return ProcessorArchitecture.Neutral;
                case InstallerArchitecture.X86:
                    return ProcessorArchitecture.X86;
                case InstallerArchitecture.X64:
                    return ProcessorArchitecture.X64;
                case InstallerArchitecture.Arm:
                    return ProcessorArchitecture.Arm;
                case InstallerArchitecture.Arm64:
                    return ProcessorArchitecture.Arm64;

                default:
                    return ProcessorArchitecture.Unknown;
            }
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
