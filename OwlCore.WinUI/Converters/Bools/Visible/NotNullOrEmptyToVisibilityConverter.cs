using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Bools.Visible
{
    /// <summary>
    /// A converter that converts checks if a string is null or empty and returns a <see cref="Visibility"/>.
    /// </summary>
    public sealed class NotNullOrEmptyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Checks if a string is null or empty, and returns a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="str">The string to null or empty check.</param>
        /// <returns><see cref="Visibility.Visible"/> if not null or empty, <see cref="Visibility.Collapsed"/> if null or empty.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Visibility Convert(string? str) => BoolToVisibilityConverter.Convert(NotNullOrEmptyToBoolConverter.Convert(str));

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                return Convert(str);
            }

            return false;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}