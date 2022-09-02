using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections;

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

        private AdvancedCollectionView _Apps;
        public AdvancedCollectionView Apps
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

        private MyAppsFilterOptions _CurrentFilter;
        public MyAppsFilterOptions CurrentFilter
        {
            get => _CurrentFilter;
            set => SetProperty(ref _CurrentFilter, value);
        }

        public void InitAppsCollection(IList apps)
        {
            Apps = new(apps)
            {
                Filter = obj =>
                    obj is AppViewModelBase app && app.ApplyFilter(CurrentFilter)
            };
            ApplyFilter(MyAppsFilterOptions.ShowAppsListEntry);
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
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                WeakReferenceMessenger.Default.Send(new SDK.Messages.ErrorMessage(ex));
                return;
            }

            // No package was found for that package family name
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            NavService.ShowHttpErrorPage(404, "That app could not be found. It may be private or not listed in the Microsoft Store.");
        }

        public void ApplyFilter(MyAppsFilterOptions options)
        {
            CurrentFilter = options;
            Apps.RefreshFilter();
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

        public virtual bool ApplyFilter(MyAppsFilterOptions _) => false;
    }

    [Flags]
    public enum MyAppsFilterOptions : byte
    {
        None = 0,
        All = byte.MaxValue,

        ShowAppsListEntry = 1 << 1,
        ShowBundles = 1 << 2,
        ShowDevMode = 1 << 3,
        ShowFramework = 1 << 4,
        ShowOptional = 1 << 5,
        ShowResource = 1 << 6,
        ShowStubs = 1 << 7,
    }
}
