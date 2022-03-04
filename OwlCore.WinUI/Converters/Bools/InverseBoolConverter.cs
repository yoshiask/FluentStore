using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Bools
{
    /// <summary>
    /// A converter that converts a given <see cref="bool"/> to its inverse.
    /// </summary>
    public sealed class InverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Gets the inverse of a bool.
        /// </summary>
        /// <param name="data">The bool to inverse.</param>
        /// <returns>An inversed bool.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Convert(bool data) => !data;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool bValue)
            {
                return Convert(bValue);
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