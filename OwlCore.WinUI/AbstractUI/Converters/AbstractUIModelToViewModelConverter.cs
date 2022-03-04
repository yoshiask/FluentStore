using Microsoft.UI.Xaml.Data;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using System;

namespace OwlCore.WinUI.AbstractUI.Converters
{
    /// <summary>
    /// Creates an <see cref="AbstractUIViewModelBase"/> from an <see cref="AbstractUIBase"/>.
    /// </summary>
    public class AbstractUIModelToViewModelConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AbstractUIViewModelBase)
                return value;

            return value switch
            {
                AbstractTextBox textBox => new AbstractTextBoxViewModel(textBox),
                AbstractDataList dataList => new AbstractDataListViewModel(dataList),
                AbstractButton button => new AbstractButtonViewModel(button),
                AbstractBoolean boolean => new AbstractBooleanViewModel(boolean),
                AbstractRichTextBlock richText => new AbstractRichTextBlockViewModel(richText),
                AbstractMultiChoice multiChoiceUIElement => new AbstractMultiChoiceViewModel(multiChoiceUIElement),
                AbstractUICollection elementGroup => new AbstractUICollectionViewModel(elementGroup),
                AbstractProgressIndicator progress => new AbstractProgressIndicatorViewModel(progress),
                AbstractColorPicker color => new AbstractColorPickerViewModel(color),

                _ => throw new NotSupportedException($"No match ViewModel was found for {value.GetType()}."),
            };
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((AbstractUIViewModelBase)value).Model;
        }
    }
}
