using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.AbstractUI.ViewModels;
using Microsoft.UI.Xaml.Data;
using OwlCore.AbstractUI.ViewModels;
using System;

namespace OwlCore.WinUI.AbstractUI.Converters
{
    internal class FormViewModelToCollectionViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AbstractFormViewModel formVm && formVm.Model is AbstractForm form)
            {
                return new AbstractUICollectionViewModel(form);
            }

            throw new ArgumentException($"{nameof(value)} must be of type {nameof(AbstractFormViewModel)}", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
