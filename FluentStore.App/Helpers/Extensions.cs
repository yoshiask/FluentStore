using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace FluentStore.Helpers
{
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

        // Yoinked from https://stackoverflow.com/a/58091583/6232957
        public static T FindControl<T>(this UIElement parent, string ControlName) where T : FrameworkElement
        {
            if (parent == null)
                return null;

            if (parent.GetType() == typeof(T) && ((T)parent).Name == ControlName)
            {
                return (T)parent;
            }
            T result = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                if (FindControl<T>(child, ControlName) != null)
                {
                    result = FindControl<T>(child, ControlName);
                    break;
                }
            }
            return result;
        }
    }
}
