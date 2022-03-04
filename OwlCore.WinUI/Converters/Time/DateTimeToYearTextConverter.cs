using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Time
{
    /// <summary>
    /// A converter that converts a given <see cref="DateTime"/> to a formatted year string.
    /// </summary>
    public class DateTimeToYearTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="DateTime"/> to a formatted year string.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to convert.</param>
        /// <returns>A formatted year string of the <see cref="DateTime"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Convert(DateTime value) => value.ToString("yyyy");

        /// <inheritdoc cref="Convert(DateTime)"/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Convert(DateTime? value) => Convert(value.HasValue ? value.Value : DateTime.MinValue);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value as DateTime?);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
