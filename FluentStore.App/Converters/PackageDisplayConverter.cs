using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class PackageDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object returnValue = value;

            if (value is IEnumerable<string> strings)
            {
                returnValue = string.Join(Environment.NewLine, strings);
            }
            else if (value is DateTimeOffset date)
            {
                returnValue = date.Date.ToShortDateString();
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
