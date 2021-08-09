using FluentStore.SDK;
using FluentStore.SDK.Images;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

            switch (image)
            {
                case FileImage _:
                    return File;

                case StreamImage _:
                    return Stream;

                case TextImage _:
                    return Text;

                case ImageBase _:
                    return Default;

                default:
                    throw new ArgumentException($"{nameof(image)} must inherit from {nameof(ImageBase)}.");
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
