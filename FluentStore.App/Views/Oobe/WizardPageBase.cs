using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentStore.Views.Oobe;

public abstract class WizardPageBase : UserControl
{
    public bool CanAdvance
    {
        get => (bool)GetValue(CanAdvanceProperty);
        set => SetValue(CanAdvanceProperty, value);
    }
    public static readonly DependencyProperty CanAdvanceProperty = DependencyProperty.Register(
        nameof(CanAdvance), typeof(bool), typeof(WizardPageBase), new PropertyMetadata(true));
}
