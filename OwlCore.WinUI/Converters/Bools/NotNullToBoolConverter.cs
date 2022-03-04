using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Bools
{
    /// <summary>
    /// A converter that converts checks null checks an object.
    /// </summary>
    public sealed class NotNullToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Checks if an object is null.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if not null, false if null</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Convert(object? obj) => obj != null;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}