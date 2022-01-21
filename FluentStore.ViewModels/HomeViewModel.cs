using FSAPI = FluentStoreAPI.FluentStoreAPI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using Microsoft.Marketplace.Storefront.Contracts;
using Garfoot.Utilities.FluentUrn;
using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.SDK.Models;
using FluentStore.SDK.Helpers;

namespace FluentStore.ViewModels
{
    public class HomeViewModel : ObservableRecipient
    {
        public HomeViewModel()
        {
            LoadFeaturedCommand = new AsyncRelayCommand(LoadFeaturedAsync);
        }

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();

        private void CarouselItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowCarousel = CarouselItems.Count > 0;
        }

        public async Task LoadFeaturedAsync()
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

                var page = await StorefrontApi.GetHomeRecommendations();

                var featured = await FSApi.GetHomePageFeaturedAsync(Windows.ApplicationModel.Package.Current.Id.Version.ToVersion());
                CarouselItems.CollectionChanged += CarouselItems_CollectionChanged;
                CarouselItems.Clear();

                for (int i = 0; i < featured.Carousel.Count; i++)
                {
                    try
                    {
                        Urn packageUrn = Urn.Parse(featured.Carousel[i]);
                        var package = await PackageService.GetPackageAsync(packageUrn);
                        CarouselItems.Add(new PackageViewModel(package));
                        if (i == 0)
                            SelectedCarouselItemIndex = i;
                    }
                    catch (System.Exception ex)
                    {
                        var logger = Ioc.Default.GetRequiredService<LoggerService>();
                        logger.Warn(ex, ex.Message);
                    }
                }
                CarouselItems.CollectionChanged -= CarouselItems_CollectionChanged;

                // Load featured packages from other sources
                FeaturedPackages = new ObservableCollection<HandlerPackageListPair>();
                await foreach (HandlerPackageListPair pair in PackageService.GetFeaturedPackagesAsync())
                    FeaturedPackages.Add(pair);
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                NavService.ShowHttpErrorPage(ex);
            }
            catch (System.Exception ex)
            {
                var logger = Ioc.Default.GetRequiredService<LoggerService>();
                logger.Warn(ex, ex.Message);
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

        private bool _ShowCarousel = true;
        public bool ShowCarousel
        {
            get => _ShowCarousel;
            set => SetProperty(ref _ShowCarousel, value);
        }

        private ObservableCollection<PackageViewModel> _CarouselItems = new();
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

        private ObservableCollection<HandlerPackageListPair> _FeaturedPackages;
        public ObservableCollection<HandlerPackageListPair> FeaturedPackages
        {
            get => _FeaturedPackages;
            set => SetProperty(ref _FeaturedPackages, value);
        }
    }
}
