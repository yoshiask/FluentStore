using FluentStore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Update;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class UpdateWindowBody : Page
{
    public UpdateWindowBody()
    {
        InitializeComponent();
    }

    public PackageViewModel ViewModel
    {
        get => (PackageViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(PackageViewModel), typeof(UpdateWindowBody), new PropertyMetadata(null));
}
