using System;
using System.Diagnostics.Contracts;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Numerical
{
    /// <summary>
    /// A converter that converts a given <see cref="double"/> to an <see cref="int"/>.
    /// </summary>
    public class DoubleToIntConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="double"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="value">The <see cref="double"/> to convert</param>
        /// <returns>The converted value.</returns>
        [Pure]
        public static int Convert(double value) => System.Convert.ToInt32(value);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert((double)value);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (int)(double)value;
        }
    }
}
