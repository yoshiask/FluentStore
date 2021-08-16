using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class NumberToStarGridLength : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Type type = value.GetType();
            if (typeof(double).IsAssignableFrom(type))
                return new GridLength((double)value, GridUnitType.Star);
            else if (typeof(int).IsAssignableFrom(type))
                return new GridLength((int)value, GridUnitType.Star);
            else
                return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is GridLength length))
                return 0;
            return length.Value;
        }
    }
}
