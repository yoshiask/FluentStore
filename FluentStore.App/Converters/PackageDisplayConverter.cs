using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace FluentStore.Converters
{
    public class PackageDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object returnValue = value;

            if (value is IEnumerable<string> strings)
            {
                TextBlock textBlock = new()
                {
                    IsTextSelectionEnabled = true,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                    TextTrimming = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis,
                };
                foreach (string s in strings)
                {
                    textBlock.Inlines.Add(new Run()
                    {
                        Text = s,
                    });
                    textBlock.Inlines.Add(new LineBreak());
                }

                // Remove last element, it will always be a
                // trailing newline
                textBlock.Inlines.RemoveAt(textBlock.Inlines.Count - 1);

                returnValue = textBlock;
            }
            else if (value is DateTimeOffset date)
            {
                returnValue = date.Date.ToShortDateString();
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
