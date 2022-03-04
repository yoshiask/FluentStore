using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace OwlCore.WinUI.Converters
{
    /// <summary>
    /// A converter that returns the <see cref="ItemClickEventArgs.ClickedItem"/> from a <see cref="ItemClickEventArgs"/> .
    /// </summary>
    public sealed class ItemClickEventArgsToClickedItemConverter : IValueConverter
    {
        /// <summary>
        /// Gets the <see cref="ItemClickEventArgs.ClickedItem"/> from a <see cref="ItemClickEventArgs"/> .
        /// </summary>
        /// <param name="args">The event args to check.</param>
        /// <returns>The clicked item, cast to <see cref="object"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Convert(ItemClickEventArgs args) => args.ClickedItem;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ItemClickEventArgs args)
                return Convert(args);

            return false;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}