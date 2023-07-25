using System;

namespace Chocolatey;

public static class Extensions
{
    public static string ToLowerString(this bool val)
    {
        return val.ToString().ToLowerInvariant();
    }

    public static TEnum ParseEnum<TEnum>(string value, bool ignoreCase = false) where TEnum : struct
    {
        return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
    }
}
