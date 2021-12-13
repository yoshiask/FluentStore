using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace FluentStore.Converters
{
    public class NameToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new FontFamily(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!typeof(FontFamily).IsAssignableFrom(value.GetType()))
                return value;
            return ((FontFamily)value).Source;
        }
    }
}
