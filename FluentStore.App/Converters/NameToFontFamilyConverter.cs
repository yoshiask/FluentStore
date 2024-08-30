using System;
using FluentStore.SDK.Models;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace FluentStore.Converters
{
    public class NameToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var str = value.ToString();

            if (SharedResources.TryGetName(str, out var resourceName)
                && App.Current.Resources.TryGetValue(resourceName, out var resourceB)
                && resourceB is FontFamily fontFamily)
            {
                return fontFamily;
            }

            return new FontFamily(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!typeof(FontFamily).IsAssignableFrom(value.GetType()))
                return value;
            return ((FontFamily)value).Source;
        }
    }
}
