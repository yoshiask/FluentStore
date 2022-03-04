using System;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters.Color
{
    /// <summary>
    /// A converter that converts a given nullable <see cref="Windows.UI.Color"/> to a <see cref="Windows.UI.Color"/> with a backup color.
    /// </summary>
    public class NullableBackupColorConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the backup color for the converter instance.
        /// </summary>
        public Windows.UI.Color BackupColor { get; set; }

        /// <summary>
        /// Converts a nullable <see cref="Windows.UI.Color"/> to a <see cref="Windows.UI.Color"/> with a backup color.
        /// </summary>
        /// <param name="value">The <see cref="Windows.UI.Color"/> to convert.</param>
        /// <param name="backup">The backup color if <paramref name="value"/> is null.</param>
        /// <returns>The converted value.</returns>
        public static Windows.UI.Color Convert(Windows.UI.Color? value, Windows.UI.Color backup) => value ?? backup;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Windows.UI.Color? color = value as Windows.UI.Color?;
            return Convert(color, BackupColor);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
