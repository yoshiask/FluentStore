using Microsoft.UI.Xaml;

namespace FluentStore.Helpers;

public static class Extensions
{
    public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
    {
        int place = Source.LastIndexOf(Find);

        if (place == -1)
            return Source;

        string result = Source.Remove(place, Find.Length).Insert(place, Replace);
        return result;
    }

    public static Visibility HideIfNull(this object obj) => obj is null ? Visibility.Collapsed : Visibility.Visible;
}
