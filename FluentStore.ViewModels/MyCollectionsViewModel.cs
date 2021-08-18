using FluentStore.SDK;
using FluentStore.SDK.Handlers;
using FluentStore.SDK.Packages;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using FluentStoreAPI.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class MyCollectionsViewModel : ObservableRecipient
    {
        public MyCollectionsViewModel()
        {
            ViewCollectionCommand = new AsyncRelayCommand(ViewCollectionAsync);
            LoadCollectionsCommand = new AsyncRelayCommand(LoadCollectionsAsync);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("My Collections"));
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();

        private ObservableCollection<PackageViewModel> _Collections = new ObservableCollection<PackageViewModel>();
        public ObservableCollection<PackageViewModel> Collections
        {
            get => _Collections;
            set => SetProperty(ref _Collections, value);
        }

        private PackageViewModel _SelectedCollection;
        public PackageViewModel SelectedCollection
        {
            get => _SelectedCollection;
            set => SetProperty(ref _SelectedCollection, value);
        }

        private IAsyncRelayCommand _ViewCollectionCommand;
        public IAsyncRelayCommand ViewCollectionCommand
        {
            get => _ViewCollectionCommand;
            set => SetProperty(ref _ViewCollectionCommand, value);
        }

        private IAsyncRelayCommand _LoadCollectionsCommand;
        public IAsyncRelayCommand LoadCollectionsCommand
        {
            get => _LoadCollectionsCommand;
            set => SetProperty(ref _LoadCollectionsCommand, value);
        }

        public async Task ViewCollectionAsync()
        {
            NavService.Navigate(SelectedCollection);
        }

        public async Task UpdateCollectionAsync(Collection newCollection)
        {
            await FSApi.UpdateCollectionAsync(UserService.CurrentFirebaseUser.LocalID, newCollection);
            await LoadCollectionsAsync();
        }

        public async Task LoadCollectionsAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            var collections = await FSApi.GetCollectionsAsync(UserService.CurrentFirebaseUser.LocalID);
            foreach (Collection collection in collections)
            {
                CollectionPackage package = new CollectionPackage(FluentStoreHandler.GetImageStatic(), collection);

                // Get the author's display name
                var authorProfile = await FSApi.GetUserProfileAsync(collection.AuthorId);
                package.Update(authorProfile);

                // Load items
                foreach (string urn in collection.Items)
                {
                    // Load the product details for each item
                    var item = await PackageService.GetPackage(Urn.Parse(urn));
                    package.Items.Add(item);
                }

                Collections.Add(new PackageViewModel(package));
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }
    }
}
