using FSAPI = FluentStoreAPI.FluentStoreAPI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts;

namespace FluentStore.ViewModels
{
    public class HomeViewModel : ObservableRecipient
    {
        public HomeViewModel()
        {
            LoadFeaturedCommand = new AsyncRelayCommand(LoadFeaturedAsync);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Home"));
        }

        public async Task LoadFeaturedAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            var featured = await FSApi.GetHomePageFeaturedAsync();
            CarouselItems.Clear();

            for (int i = 0; i < featured.Carousel.Count; i++)
            {
                string productId = featured.Carousel[i];
                var product = (await StorefrontApi.GetProduct(productId)).Payload;
                CarouselItems.Add(new PackageViewModel(new MicrosoftStorePackage(product)));
                if (i == 0 || (i == 1 && featured.Carousel.Count >= 3))
                    SelectedCarouselItemIndex = i;
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        private readonly StorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<StorefrontApi>();
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();

        private IAsyncRelayCommand _LoadFeaturedCommand;
        public IAsyncRelayCommand LoadFeaturedCommand
        {
            get => _LoadFeaturedCommand;
            set => SetProperty(ref _LoadFeaturedCommand, value);
        }

        private ObservableCollection<PackageViewModel> _CarouselItems = new ObservableCollection<PackageViewModel>();
        public ObservableCollection<PackageViewModel> CarouselItems
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

        private PackageViewModel _SelectedCarouselItem;
        public PackageViewModel SelectedCarouselItem
        {
            get => _SelectedCarouselItem;
            set => SetProperty(ref _SelectedCarouselItem, value);
        }
    }
}
