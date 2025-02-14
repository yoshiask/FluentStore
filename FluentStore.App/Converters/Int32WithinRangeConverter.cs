using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentStore.Converters;

public class Int32WithinRangeConverter : DependencyObject, IValueConverter
{
    public static readonly DependencyProperty LowerBoundProperty = DependencyProperty.Register(
            nameof(LowerBound), typeof(int), typeof(Int32WithinRangeConverter), new PropertyMetadata(int.MinValue));

    public static readonly DependencyProperty UpperBoundProperty = DependencyProperty.Register(
            nameof(UpperBound), typeof(int), typeof(Int32WithinRangeConverter), new PropertyMetadata(int.MaxValue));

    public static readonly DependencyProperty NegateProperty = DependencyProperty.Register(
            nameof(Negate), typeof(bool), typeof(Int32WithinRangeConverter), new PropertyMetadata(false));

    public int LowerBound
    {
        get => (int)GetValue(LowerBoundProperty);
        set => SetValue(LowerBoundProperty, value);
    }

    public int UpperBound
    {
        get => (int)GetValue(UpperBoundProperty);
        set => SetValue(UpperBoundProperty, value);
    }

    public bool Negate
    {
        get => (bool)GetValue(NegateProperty);
        set => SetValue(NegateProperty, value);
    }

    public object Convert(object obj, Type targetType, object parameter, string language)
    {
        var value = System.Convert.ToInt32(obj);
        var isWithinRange = value >= LowerBound && value <= UpperBound;
        return Negate ? !isWithinRange : isWithinRange;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
