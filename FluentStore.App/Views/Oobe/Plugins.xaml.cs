using FluentStore.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Oobe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Plugins : WizardPageBase
    {
        public Plugins()
        {
            this.InitializeComponent();

            CanAdvance = false;
            ViewModel.SelectedPackages.CollectionChanged += SelectedPackages_CollectionChanged;
        }

        public PackageManagerViewModel ViewModel { get; } = new();

        private void SelectedPackages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CanAdvance = ViewModel.SelectedPackages.Count > 0;
        }
    }
}
