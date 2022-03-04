using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace OwlCore.WinUI.Converters.Color
{
    /// <summary>
    /// A converter that converts a given <see cref="Windows.UI.Color"/> to a <see cref="SolidColorBrush"/>.
    /// </summary>
    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="Windows.UI.Color"/> to a <see cref="SolidColorBrush"/>.
        /// </summary>
        /// <param name="value">The <see cref="Windows.UI.Color"/> to convert</param>
        /// <returns>The converted value.</returns>
        public static SolidColorBrush Convert(Windows.UI.Color? value) => new SolidColorBrush(value ?? Microsoft.UI.Colors.Transparent);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value as Windows.UI.Color?);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((SolidColorBrush)value).Color;
        }
    }
}
