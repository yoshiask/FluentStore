using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class MyAppsViewModel : ObservableRecipient
    {
        public MyAppsViewModel()
        {
            ViewAppCommand = new AsyncRelayCommand(ViewAppAsync);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("My Apps"));
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        private ObservableCollection<AppViewModelBase> _Apps;
        public ObservableCollection<AppViewModelBase> Apps
        {
            get => _Apps;
            set => SetProperty(ref _Apps, value);
        }

        private AppViewModelBase _SelectedApp;
        public AppViewModelBase SelectedApp
        {
            get => _SelectedApp;
            set => SetProperty(ref _SelectedApp, value);
        }

        private IAsyncRelayCommand _ViewAppCommand;
        public IAsyncRelayCommand ViewAppCommand
        {
            get => _ViewAppCommand;
            set => SetProperty(ref _ViewAppCommand, value);
        }

        private bool _IsLoadingMyApps;
        public bool IsLoadingMyApps
        {
            get => _IsLoadingMyApps;
            set => SetProperty(ref _IsLoadingMyApps, value);
        }

        public async Task ViewAppAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            try
            {
                // Get the full product details
                var package = await PackageService.GetPackageAsync(
                    Urn.Parse($"urn:win-modern-package:{SelectedApp.PackageFamilyName}"));
                if (package != null)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavService.Navigate("PackageView", package);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                WeakReferenceMessenger.Default.Send(new SDK.Messages.ErrorMessage(ex));
                return;
            }

            // No package was found for that package family name
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            NavService.ShowHttpErrorPage(404, "That app could not be found. It may be private or not listed in the Microsoft Store.");
        }
    }

    public class AppViewModelBase : ObservableObject
    {
        public AppViewModelBase()
        {
            LaunchCommand = new AsyncRelayCommand(LaunchAsync);
        }

        private string _DisplayName;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetProperty(ref _DisplayName, value);
        }

        private string _PackageFamilyName;
        public string PackageFamilyName
        {
            get => _PackageFamilyName;
            set => SetProperty(ref _PackageFamilyName, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        private System.IO.Stream _Icon;
        public System.IO.Stream Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        private IAsyncRelayCommand _LaunchCommand;
        public IAsyncRelayCommand LaunchCommand
        {
            get => _LaunchCommand;
            set => SetProperty(ref _LaunchCommand, value);
        }

        public virtual async Task<bool> LaunchAsync() => false;
    }
}
