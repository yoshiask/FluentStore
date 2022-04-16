using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Packages;
using System.Collections.Generic;
using System.Linq;

namespace FluentStore.ViewModels
{
    public class MyCollectionsViewModel : ObservableRecipient
    {
        public MyCollectionsViewModel()
        {
            ViewCollectionCommand = new AsyncRelayCommand(ViewCollectionAsync);
            LoadCollectionsCommand = new AsyncRelayCommand(LoadCollectionsAsync);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Collections"));
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        private ObservableCollection<PackageViewModel> _collections = new();
        private PackageViewModel _selectedCollection;
        private IAsyncRelayCommand _viewCollectionCommand;
        private IAsyncRelayCommand _loadCollectionsCommand;
        private bool _showNewCollectionTip = false;

        public ObservableCollection<PackageViewModel> Collections
        {
            get => _collections;
            set => SetProperty(ref _collections, value);
        }
        
        public PackageViewModel SelectedCollection
        {
            get => _selectedCollection;
            set => SetProperty(ref _selectedCollection, value);
        }

        public IAsyncRelayCommand ViewCollectionCommand
        {
            get => _viewCollectionCommand;
            set => SetProperty(ref _viewCollectionCommand, value);
        }

        public IAsyncRelayCommand LoadCollectionsCommand
        {
            get => _loadCollectionsCommand;
            set => SetProperty(ref _loadCollectionsCommand, value);
        }

        public bool ShowNewCollectionTip
        {
            get => _showNewCollectionTip;
            set => SetProperty(ref _showNewCollectionTip, value);
        }

        public async Task ViewCollectionAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            try
            {
                if (SelectedCollection.Package.Status.IsLessThan(PackageStatus.Details))
                {
                    // Fetch more detailed info
                    SelectedCollection.Package = await PackageService.GetPackageAsync(SelectedCollection.Package.Urn, PackageStatus.Details);
                }

                NavService.Navigate(SelectedCollection);
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new SDK.Messages.ErrorMessage(ex, SelectedCollection.Package));
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public async Task UpdateCollectionAsync(PackageHandlerBase handler, PackageBase newCollection)
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
            try
            {
                await handler.SavePackageAsync(newCollection);
                await LoadCollectionsAsync();
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new SDK.Messages.ErrorMessage(ex, newCollection));
            }
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public async Task LoadCollectionsAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            Collections.Clear();
            try
            {
                var collections = await PackageService.GetCollectionsAsync();
                foreach (PackageBase collection in collections)
                {
                    Collections.Add(new PackageViewModel(collection));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new SDK.Messages.ErrorMessage(ex));
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));

            ShowNewCollectionTip = Collections.Count <= 0;
        }

        public IEnumerable<PackageHandlerBase> GetPackageHandlersForNewCollections()
        {
            return PackageService.PackageHandlers.Where(ph => ph.IsEnabled && ph.CanCreateCollection());
        }
    }
}
