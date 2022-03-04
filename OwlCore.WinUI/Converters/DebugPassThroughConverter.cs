using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace OwlCore.WinUI.Converters
{
    /// <summary>
    /// A converter used for debugging the data passed by a binding.
    /// </summary>
    public sealed class DebugPassThroughConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
