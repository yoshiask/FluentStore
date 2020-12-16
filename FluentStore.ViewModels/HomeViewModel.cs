using FSAPI = FluentStoreAPI.FluentStoreAPI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MicrosoftStore;
using MicrosoftStore.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class HomeViewModel : ObservableRecipient
    {
        public HomeViewModel()
        {
            LoadFeaturedCommand = new AsyncRelayCommand(LoadFeaturedAsync);
        }

        public async Task LoadFeaturedAsync()
        {
            var culture = CultureInfo.CurrentUICulture;
            var region = new RegionInfo(culture.LCID);

            var featured = await FSApi.GetHomePageFeaturedAsync();
            CarouselItems.Clear();

            for (int i = 0; i < featured.Carousel.Count; i++)
            {
                string productId = featured.Carousel[i];
                var product = (await StorefrontApi.GetProduct(productId, region.TwoLetterISORegionName, culture.Name))
                    .Convert<ProductDetails>().Payload;
                CarouselItems.Add(new ProductDetailsViewModel() { Product = product });
                if (i == 0 || (i == 1 && featured.Carousel.Count >= 3))
                    SelectedCarouselItemIndex = i;
            }
        }

        private readonly IStorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<IStorefrontApi>();
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();

        private IAsyncRelayCommand _LoadFeaturedCommand;
        public IAsyncRelayCommand LoadFeaturedCommand
        {
            get => _LoadFeaturedCommand;
            set => SetProperty(ref _LoadFeaturedCommand, value);
        }

        private ObservableCollection<ProductDetailsViewModel> _CarouselItems = new ObservableCollection<ProductDetailsViewModel>();
        public ObservableCollection<ProductDetailsViewModel> CarouselItems
        {
            get => _CarouselItems;
            set => SetProperty(ref _CarouselItems, value);
        }

        private int _SelectedCarouselItemIndex = -1;
        public int SelectedCarouselItemIndex
        {
            get => _SelectedCarouselItemIndex;
            set => SetProperty(ref _SelectedCarouselItemIndex, value);
        }

        private ProductDetailsViewModel _SelectedCarouselItem;
        public ProductDetailsViewModel SelectedCarouselItem
        {
            get => _SelectedCarouselItem;
            set => SetProperty(ref _SelectedCarouselItem, value);
        }
    }
}
