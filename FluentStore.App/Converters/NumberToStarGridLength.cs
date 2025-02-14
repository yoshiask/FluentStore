using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class NumberToStarGridLength : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                double d when d >= 0 => new GridLength(d, GridUnitType.Star),
                int i when i >= 0 => new GridLength(i, GridUnitType.Star),
                _ => new GridLength(0)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is GridLength length))
                return 0;
            return length.Value;
        }
    }
}
