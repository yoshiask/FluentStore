using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Time
{
    /// <summary>
    /// A converter that converts a given <see cref="TimeSpan"/> to a natural time format string.
    /// </summary>
    public sealed class TimeSpanToTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to a formatted string.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> to convert.</param>
        /// <returns>A formatted string of the <see cref="TimeSpan"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Convert(TimeSpan value)
        {
            // TODO: Make more rigorous cases
            if (value.Hours > 0) return value.ToString(@"h\:mm\:ss");
            else return value.ToString(@"m\:ss");
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan timeSpan)
            {
                return Convert(timeSpan);
            }

            return Convert(TimeSpan.Zero);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
