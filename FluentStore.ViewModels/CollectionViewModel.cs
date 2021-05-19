using FluentStore.Services;
using FluentStoreAPI.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
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
            ViewItemCommand = new RelayCommand(ViewItem);
            LoadItemsCommand = new AsyncRelayCommand(LoadItemsAsync);
        }
        public CollectionViewModel(Collection collection)
        {
            ViewItemCommand = new RelayCommand(ViewItem);
            LoadItemsCommand = new AsyncRelayCommand(LoadItemsAsync);
            Collection = collection;
        }

        private readonly IStorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<IStorefrontApi>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();

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

        private ObservableCollection<ProductDetailsViewModel> _Items = new ObservableCollection<ProductDetailsViewModel>();
        public ObservableCollection<ProductDetailsViewModel> Items
        {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }

        private ProductDetailsViewModel _SelectedItem;
        public ProductDetailsViewModel SelectedItem
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

        public void ViewItem()
        {
            if (SelectedItem == null)
                return;

            LoadItemsCommand.Cancel();
            NavService.Navigate(SelectedItem);
        }

        public async Task LoadItemsAsync()
        {
            // Get the author's display name
            var authorProfile = await FSApi.GetUserProfileAsync(Collection.AuthorId);
            AuthorName = authorProfile.DisplayName;

            // Load items
            var culture = CultureInfo.CurrentUICulture;
            var region = new RegionInfo(culture.LCID);

            Items.Clear();
            foreach (string productId in Collection.Items)
            {
                // Load the product details for each item
                var product = (await StorefrontApi.GetProduct(productId, region.TwoLetterISORegionName, culture.Name))
                    .Convert<ProductDetails>().Payload;
                Items.Add(new ProductDetailsViewModel(product));
            }
        }
    }
}
