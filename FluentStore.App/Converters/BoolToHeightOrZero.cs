using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class BoolToHeightOrZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
            {
                Type type = parameter.GetType();
                GridLength l = GridLength.Auto;
                if (typeof(double).IsAssignableFrom(type))
                    l = new GridLength((double)parameter, GridUnitType.Star);
                else if (typeof(int).IsAssignableFrom(type))
                    l = new GridLength((int)parameter, GridUnitType.Star);
                else if (parameter is string paraStr)
                    l = new GridLength(double.Parse(paraStr), GridUnitType.Star);
                return l;
            }
            else
            {
                return new GridLength(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is GridLength length))
                return 0;
            return length.Value;
        }
    }
}
