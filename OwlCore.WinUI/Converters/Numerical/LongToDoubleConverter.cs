using System;
using System.Diagnostics.Contracts;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Numerical
{
    /// <summary>
    /// A converter that converts a given <see cref="long"/> to a <see cref="double"/>.
    /// </summary>
    public class LongToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="long"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="value">The <see cref="long"/> to convert</param>
        /// <returns>The converted value.</returns>
        [Pure]
        public static double Convert(long value) => value;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert((long)value);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double)value;
        }
    }
}
