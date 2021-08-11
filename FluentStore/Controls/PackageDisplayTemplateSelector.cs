using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentStore.Controls
{
    public class PackageDisplayTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate DateTime { get; set; }
        public DataTemplate DateTimeOffset { get; set; }
        public DataTemplate Uri { get; set; }
        public DataTemplate Enumerable { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"SelectTemplateCore({item ?? "null"})");
#endif
            if (item == null)
                return Default;

            Type type = item.GetType();
            if (typeof(DateTime).IsAssignableFrom(type))
                return DateTime;
            else if (typeof(DateTimeOffset).IsAssignableFrom(type))
                return DateTimeOffset;
            else if (typeof(Uri).IsAssignableFrom(type))
                return Uri;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return Enumerable;
            else
                return Default;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        public static UIElement CreateElement(object item)
        {
            if (item == null)
                return new TextBlock { Text = "{null}" };

            UIElement rootElem;
            Type type = item.GetType();
            if (typeof(DateTime).IsAssignableFrom(type))
            {
                DateTime dateTime = (DateTime)item;
                rootElem = new TextBlock
                {
                    Text = dateTime.ToShortDateString()
                };
            }
            else if (typeof(DateTimeOffset).IsAssignableFrom(type))
            {
                DateTimeOffset dateTimeOffset = (DateTimeOffset)item;
                rootElem = new TextBlock
                {
                    Text = dateTimeOffset.ToString("d/M/yyy")
                };
            }
            else if (typeof(Uri).IsAssignableFrom(type))
            {
                Uri uri = (Uri)item;
                rootElem = new HyperlinkButton
                {
                    NavigateUri = uri,
                    Content = uri.AbsoluteUri
                };
            }
            else if (typeof(string).IsAssignableFrom(type))
            {
                string str = (string)item;
                rootElem = new TextBlock
                {
                    Text = str
                };
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                IEnumerable enumerable = (IEnumerable)item;
                var enumerableStack = new StackPanel();
                foreach (object obj in enumerable)
                {
                    enumerableStack.Children.Add(CreateElement(obj));
                }
                rootElem = enumerableStack;
            }
            else
            {
                rootElem = new TextBlock { Text = item.ToString() };
            }

            return rootElem;
        }
    }
}
