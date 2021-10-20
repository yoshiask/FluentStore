using FluentStore.ViewModels;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class PlatWindowsEnumToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not PackageViewModel vm)
                return Visibility.Collapsed;
            PlatWindows platform = (PlatWindows)Enum.Parse(typeof(PlatWindows), (string)parameter);
            return vm.Package.CanBeInstalled() ? Visibility.Visible : Visibility.Collapsed;
            //package.SupportsPlatform(platform)
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
