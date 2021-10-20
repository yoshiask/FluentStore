using FluentStore.SDK;
using FluentStore.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Controls
{
    public class PackageDisplayTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate DateTime { get; set; }
        public DataTemplate DateTimeOffset { get; set; }
        public DataTemplate Uri { get; set; }
        public DataTemplate PackageEnumerable { get; set; }
        public DataTemplate Enumerable { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item == null)
                return Default;

            Type type = item.GetType();
            if (typeof(DateTime).IsAssignableFrom(type))
                return DateTime;
            else if (typeof(DateTimeOffset).IsAssignableFrom(type))
                return DateTimeOffset;
            else if (typeof(Uri).IsAssignableFrom(type))
                return Uri;
            else if (typeof(IEnumerable<PackageBase>).IsAssignableFrom(type))
                return PackageEnumerable;
            else if (typeof(IEnumerable<PackageViewModel>).IsAssignableFrom(type))
                return PackageEnumerable;
            else if (typeof(string).IsAssignableFrom(type))
                return Default;
            else if (typeof(IEnumerable).IsAssignableFrom(type))
                return Enumerable;
            else
                return Default;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
