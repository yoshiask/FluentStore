using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Time.Numerical
{
    /// <summary>
    /// A converter that converts a given <see cref="long"/> to a <see cref="TimeSpan"/> then to a natural time format string.
    /// </summary>
    public sealed class LongToTimeSpanTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="long"/> to a <see cref="TimeSpan"/> formatted string.
        /// </summary>
        /// <param name="value">The <see cref="long"/> to convert.</param>
        /// <returns>A formatted string of the <see cref="long"/> as a <see cref="TimeSpan"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Convert(long value) => TimeSpanToTextConverter.Convert(LongToTimeSpanConverter.Convert(value));

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            long lValue = 0;
            switch (value)
            {
                case long l:
                    lValue = l;
                    break;
                case double d:
                    lValue = (long)d;
                    break;
                case int i:
                    lValue = i;
                    break;
                case float f:
                    lValue = (long)f;
                    break;
            }
            return Convert(lValue);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
