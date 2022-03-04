using System;
using System.Diagnostics.Contracts;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Numerical
{
    /// <summary>
    /// A converter that converts a given <see cref="string"/> to an <see cref="int"/>.
    /// </summary>
    public class StringToIntConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="string"/> to an <see cref="int"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> to convert</param>
        /// <returns>The converted value.</returns>
        [Pure]
        public static int Convert(string value) => int.Parse(value);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert((string)value);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (double)value;
        }
    }
}
