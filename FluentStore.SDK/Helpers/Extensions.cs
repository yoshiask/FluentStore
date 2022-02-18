using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        /// <summary>
        /// Acts exactly like <see cref="string.Join(char, string?[])"/>, but for any <see cref="IList{T}"/>.
        /// </summary>
        public static void Join<T>(this IList<T> list, T insert)
        {
            for (int i = 0; i < list.Count - 1; i += 2)
            {
                list.Insert(i + 1, insert);
            }
        }

        public static Version ToVersion(this Windows.ApplicationModel.PackageVersion packageVersion)
        {
            return new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
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

        public static bool Contains(this string str, StringComparison comparisonType, params string[] matches)
        {
            bool contains = false;
            foreach (string match in matches)
                contains |= str.Contains(match, comparisonType);
            return contains;
        }

        // TODO: Does this really belong here?
        public static bool TryParseUrn(string str, [NotNullWhen(true)] out Urn urn)
        {
            try
            {
                urn = Urn.Parse(str);
                return true;
            }
            catch
            {
                urn = default;
                return false;
            }
        }

        public static WindowsPlatform ParseWindowsPlatform(string str)
        {
            int dotIdx = str.LastIndexOf('.');
            if (dotIdx >= 0)
                str = str[(dotIdx + 1)..];

            if (Enum.TryParse<WindowsPlatform>(str, out var platWindows))
                return platWindows;
            else
                return WindowsPlatform.Unknown;
        }
    }
}
