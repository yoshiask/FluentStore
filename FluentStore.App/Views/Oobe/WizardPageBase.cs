using FluentStore.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Views.Oobe;

public abstract class WizardPageBase : UserControl
{
    public WizardPageBase(StartupWizardViewModel viewModel)
    {
        Wizard = viewModel;
    }

    protected StartupWizardViewModel Wizard { get; }

    public bool CanAdvance
    {
        get => (bool)GetValue(CanAdvanceProperty);
        set => SetValue(CanAdvanceProperty, value);
    }
    public static readonly DependencyProperty CanAdvanceProperty = DependencyProperty.Register(
        nameof(CanAdvance), typeof(bool), typeof(WizardPageBase), new PropertyMetadata(true));

    public virtual void OnNavigatingFrom() { }
}
