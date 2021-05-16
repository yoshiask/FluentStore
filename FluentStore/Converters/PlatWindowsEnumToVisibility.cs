using FluentStore.ViewModels;
using MicrosoftStore.Enums;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class PlatWindowsEnumToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is ProductDetailsViewModel product))
                return Visibility.Collapsed;
            PlatWindows platform = (PlatWindows)Enum.Parse(typeof(PlatWindows), (string)parameter);
            return product.SupportsPlatform(platform) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
