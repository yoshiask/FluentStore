using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Time.Numerical
{
    /// <summary>
    /// A converter that converts a given <see cref="long"/> to a <see cref="TimeSpan"/>.
    /// </summary>
    public sealed class DoubleToTimeSpanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="long"/> to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> to convert.</param>
        /// <returns>A formatted string of the <see cref="TimeSpan"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Convert(double value) => TimeSpan.FromMilliseconds(value);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double timeSpan)
            {
                return Convert(timeSpan);
            }

            return Convert(0);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}