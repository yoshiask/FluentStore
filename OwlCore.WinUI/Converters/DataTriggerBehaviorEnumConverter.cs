using System;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters
{
    /// <summary>
    /// A converter that provides a workaround for DataTriggerBehavior not working with enums.
    /// </summary>
    /// <remarks>
    /// <see href="https://stackoverflow.com/questions/23728327/datatriggerbehavior-doesnt-work-with-enum">From StackOverflow</see>: "Apparently it doesn’t account for enum types and falls back to some logic which recreates the xaml string for the enum. Parses it as ContentControl and passes back its content. Unfortunately during this step it loses the enum type information and subsequent type casting is not valid."
    /// </remarks>
    public sealed class DataTriggerBehaviorEnumConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
