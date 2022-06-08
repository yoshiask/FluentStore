using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using CommunityToolkit.Diagnostics;
using OwlCore.WinUI.AbstractUI.Controls;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.AbstractUI.Themes
{
    /// <summary>
    /// Selects the template that is used for an <see cref="AbstractButton"/> based on the <see cref="AbstractButton.Type"/>.
    /// </summary>
    public class AbstractButtonTemplateSelector : IValueConverter
    {
        /// <summary>
        /// The data template used to a display an <see cref="AbstractButton"/> with a generic style.
        /// </summary>
        public Style? GenericStyle { get; set; }

        /// <summary>
        /// The data template used to a display an <see cref="AbstractButton"/> with a confirmation style.
        /// </summary>
        public Style? ConfirmStyle { get; set; }

        /// <summary>
        /// The data template used to a display an <see cref="AbstractButton"/> with a deletion style.
        /// </summary>
        public Style? DeleteStyle { get; set; }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = AbstractButtonType.Generic;

            if (value is AbstractButtonType vType)
                type = vType;
            if (value is AbstractButtonViewModel buttonViewModel)
                type = buttonViewModel.Type;
            else if (value is AbstractButton button)
                type = button.Type;

            return type switch
            {
                AbstractButtonType.Generic => GenericStyle ?? ThrowHelper.ThrowArgumentNullException<Style>(),
                AbstractButtonType.Confirm => ConfirmStyle ?? GenericStyle ?? ThrowHelper.ThrowArgumentNullException<Style>(),
                AbstractButtonType.Cancel => DeleteStyle ?? GenericStyle ?? ThrowHelper.ThrowArgumentNullException<Style>(),
                _ => throw new NotImplementedException(),
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
