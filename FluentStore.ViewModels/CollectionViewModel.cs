using FluentStore.SDK.Packages;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using FluentStoreAPI.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using MicrosoftStore;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class CollectionViewModel : ObservableRecipient
    {
        public CollectionViewModel()
        {
            SetUpCommands();
        }
        public CollectionViewModel(Collection collection)
        {
            SetUpCommands();
            Collection = collection;
        }

        private void SetUpCommands()
        {
            ViewItemCommand = new RelayCommand(ViewItem);
            LoadItemsCommand = new AsyncRelayCommand(LoadItemsAsync);
            UpdateCollectionCommand = new AsyncRelayCommand<Collection>(UpdateCollectionAsync);
        }

        private readonly StorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<StorefrontApi>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();
        private readonly UserService UserService = Ioc.Default.GetRequiredService<UserService>();

        private Collection _Collection;
        public Collection Collection
        {
            get => _Collection;
            set => SetProperty(ref _Collection, value);
        }

        private string _AuthorName;
        public string AuthorName
        {
            get => _AuthorName;
            set => SetProperty(ref _AuthorName, value);
        }

        private ObservableCollection<PackageViewModel> _Items = new ObservableCollection<PackageViewModel>();
        public ObservableCollection<PackageViewModel> Items
        {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }

        private PackageViewModel _SelectedItem;
        public PackageViewModel SelectedItem
        {
            get => _SelectedItem;
            set => SetProperty(ref _SelectedItem, value);
        }

        private IRelayCommand _ViewItemCommand;
        public IRelayCommand ViewItemCommand
        {
            get => _ViewItemCommand;
            set => SetProperty(ref _ViewItemCommand, value);
        }

        private IAsyncRelayCommand<object> _DownloadCommand;
        public IAsyncRelayCommand<object> DownloadCommand
        {
            get => _DownloadCommand;
            set => SetProperty(ref _DownloadCommand, value);
        }

        private IAsyncRelayCommand<object> _InstallCommand;
        public IAsyncRelayCommand<object> InstallCommand
        {
            get => _InstallCommand;
            set => SetProperty(ref _InstallCommand, value);
        }

        private IAsyncRelayCommand<object> _DeleteCommand;
        public IAsyncRelayCommand<object> DeleteCommand
        {
            get => _DeleteCommand;
            set => SetProperty(ref _DeleteCommand, value);
        }

        private IAsyncRelayCommand _LoadItemsCommand;
        public IAsyncRelayCommand LoadItemsCommand
        {
            get => _LoadItemsCommand;
            set => SetProperty(ref _LoadItemsCommand, value);
        }

        private IAsyncRelayCommand<Collection> _UpdateCollectionCommand;
        public IAsyncRelayCommand<Collection> UpdateCollectionCommand
        {
            get => _UpdateCollectionCommand;
            set => SetProperty(ref _UpdateCollectionCommand, value);
        }

        public void ViewItem()
        {
            if (SelectedItem == null)
                return;

            LoadItemsCommand.Cancel();
            NavService.Navigate(SelectedItem);
        }

        public async Task LoadItemsAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            // Get the author's display name
            var authorProfile = await FSApi.GetUserProfileAsync(Collection.AuthorId);
            AuthorName = authorProfile.DisplayName;

            // Load items
            Items.Clear();
            foreach (string productId in Collection.Items)
            {
                // Load the product details for each item
                var product = (await StorefrontApi.GetProduct(productId)).Payload;
                Items.Add(new PackageViewModel(new MicrosoftStorePackage(product)));
            }
            
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public async Task UpdateCollectionAsync(Collection newCollection)
        {
            await FSApi.UpdateCollectionAsync(UserService.CurrentFirebaseUser.LocalID, newCollection);
            Collection = await FSApi.GetCollectionAsync(UserService.CurrentFirebaseUser.LocalID, newCollection.Id.ToString());
        }
    }
}
