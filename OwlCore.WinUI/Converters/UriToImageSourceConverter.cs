using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace OwlCore.WinUI.Converters
{
    /// <summary>
    /// A converter that converts a given <see cref="Uri"/> to a <see cref="BitmapImage"/>.
    /// </summary>
    public sealed class UriToImageSourceConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="Uri"/> or url string to a <see cref="BitmapImage"/>.
        /// </summary>
        /// <param name="value">The uri or url string.</param>
        /// <returns>A <see cref="BitmapImage"/> with the url as a source.</returns>
        public static BitmapImage? Convert(object value)
        {
            Uri? uri = null;

            if (value is Uri)
            {
                uri = value as Uri;
            }
            else if (value is string stringUri)
            {
                Uri.TryCreate(stringUri, UriKind.Absolute, out uri);
            }

            if (uri != null)
            {
                return new BitmapImage(uri);
            }

            return null;
        }

        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is BitmapImage bitmap)
            {
                return bitmap.UriSource;
            }

            return null;
        }
    }
}
