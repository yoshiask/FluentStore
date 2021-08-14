using FluentStore.SDK;
using FluentStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace FluentStore.Converters
{
    public class PackageBaseEnumerableToViewModelEnumerable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!typeof(IEnumerable<PackageBase>).IsAssignableFrom(value.GetType()))
                return value;
            return new ObservableCollection<PackageViewModel>(((IEnumerable<PackageBase>)value).Select(pb => new PackageViewModel(pb)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!typeof(IEnumerable<PackageViewModel>).IsAssignableFrom(value.GetType()))
                return value;
            return new ObservableCollection<PackageBase>(((IEnumerable<PackageViewModel>)value).Select(pvm => pvm.Package));
        }
    }
}
