using FluentStore.Helpers;
using FluentStore.SDK.Plugins;
using FluentStore.SDK.Plugins.NuGet;
using FluentStore.ViewModels;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Oobe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Plugins : WizardPageBase
    {
        private readonly StartupWizardViewModel _wizard;
        private readonly FluentStoreNuGetProject _nugetProject;

        public Plugins(StartupWizardViewModel wizard, PluginLoader pluginLoader) : base(wizard)
        {
            this.InitializeComponent();

            _nugetProject = pluginLoader.Project;
            ViewModel = new();

            UpdateCanAdvance();
            ViewModel.SelectedPackages.CollectionChanged += SelectedPackages_CollectionChanged;
        }

        public PackageManagerViewModel ViewModel { get; }

        private void SelectedPackages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCanAdvance();
        }

        private void UpdateCanAdvance()
        {
            CanAdvance = ViewModel.SelectedPackages.Count > 0
                || _nugetProject.Entries.Values.Where(p => p.InstallStatus == PluginInstallStatus.Completed).Any();
        }

        public override void OnNavigatingFrom()
        {
            Wizard.PluginsToInstall = ViewModel.SelectedPackages.ToList();
        }
    }
}
