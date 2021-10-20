using FluentStore.SDK.Images;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Controls
{
    public class ImageBaseTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate File { get; set; }
        public DataTemplate Stream { get; set; }
        public DataTemplate Text { get; set; }

        protected override DataTemplate SelectTemplateCore(object image)
        {
            if (image == null)
                return Default;

            return image switch
            {
                FileImage => File,
                StreamImage => Stream,
                TextImage => Text,
                ImageBase => Default,
                _ => throw new ArgumentException($"{nameof(image)} must inherit from {nameof(ImageBase)}."),
            };
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
