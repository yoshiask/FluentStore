using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MicrosoftStore;
using MicrosoftStore.Models;
using StoreLib.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class SearchResultsViewModel : ObservableRecipient
    {
        public SearchResultsViewModel()
        {
            PopulateProductDetailsCommand = new AsyncRelayCommand(PopulateProductDetailsAsync);
            GetSuggestionsCommand = new AsyncRelayCommand(GetSuggestionsAsync);
        }
        public SearchResultsViewModel(string query)
        {
            PopulateProductDetailsCommand = new AsyncRelayCommand(PopulateProductDetailsAsync);
            GetSuggestionsCommand = new AsyncRelayCommand(GetSuggestionsAsync);
            Query = query;
        }

        private readonly IStorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<IStorefrontApi>();
        private readonly IMSStoreApi MSStoreApi = Ioc.Default.GetRequiredService<IMSStoreApi>();

        private string _Query;
        public string Query
        {
            get => _Query;
            set
            {
                SetProperty(ref _Query, value);
                GetSuggestionsCommand.Execute(null);
            }
        }

        private ObservableCollection<StoreLib.Models.Product> _Products;
        public ObservableCollection<StoreLib.Models.Product> Products
        {
            get => _Products;
            set
            {
                SetProperty(ref _Products, value);
                PopulateProductDetailsCommand.Execute(null);
            }
        }

        private ObservableCollection<ProductDetailsViewModel> _ProductDetails = new ObservableCollection<ProductDetailsViewModel>();
        public ObservableCollection<ProductDetailsViewModel> ProductDetails
        {
            get => _ProductDetails;
            set => SetProperty(ref _ProductDetails, value);
        }

        private IAsyncRelayCommand _PopulateProductDetailsCommand;
        public IAsyncRelayCommand PopulateProductDetailsCommand
        {
            get => _PopulateProductDetailsCommand;
            set => SetProperty(ref _PopulateProductDetailsCommand, value);
        }

        private IAsyncRelayCommand _GetSuggestionsCommand;
        public IAsyncRelayCommand GetSuggestionsCommand
        {
            get => _GetSuggestionsCommand;
            set => SetProperty(ref _GetSuggestionsCommand, value);
        }

        public async Task PopulateProductDetailsAsync()
        {
            var culture = CultureInfo.CurrentUICulture;
            var region = new RegionInfo(culture.LCID);
            ProductDetails.Clear();

            foreach (StoreLib.Models.Product product in Products)
            {
                // Get the full product details
                var item = await StorefrontApi.GetProduct(product.ProductId, region.TwoLetterISORegionName, culture.Name);
                var candidate = item.Convert<ProductDetails>().Payload;
                if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                {
                    ProductDetails.Add(new ProductDetailsViewModel(candidate));
                }
            }
        }

        public async Task GetSuggestionsAsync()
        {
            var dcat = new DisplayCatalogHandler(StoreLib.Models.DCatEndpoint.Production, new Locale(CultureInfo.CurrentUICulture, true));
            var dcatSearch = await dcat.SearchDCATAsync(Query, StoreLib.Models.DeviceFamily.Desktop);
            var products = new ObservableCollection<StoreLib.Models.Product>();
            foreach (var result in dcatSearch.Results)
            {
                foreach (var product in result.Products)
                {
                    products.Add(product);
                }
            }
            Products = products;
        }
    }
}
