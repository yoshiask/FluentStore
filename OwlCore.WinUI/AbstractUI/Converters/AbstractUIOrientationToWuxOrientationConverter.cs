using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using OwlCore.AbstractUI.Models;

namespace OwlCore.WinUI.AbstractUI.Converters
{
    /// <summary>
    /// Converts a <see cref="PreferredOrientation"/> to <see cref="Orientation"/>.
    /// </summary>
    public class AbstractUIOrientationToWuxOrientationConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (Orientation)(PreferredOrientation)value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (PreferredOrientation)(Orientation)value;
        }
    }
}