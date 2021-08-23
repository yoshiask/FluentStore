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

        public static string GetExtension(this InstallerType type) => type.ToString().ToLower();
    }
}
