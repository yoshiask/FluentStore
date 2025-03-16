using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Models;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using FluentStoreAPI;
using Garfoot.Utilities.FluentUrn;
using OwlCore.Extensions;

namespace FluentStore.ViewModels
{
    public class HomeViewModel : ObservableRecipient
    {
        public HomeViewModel()
        {
            LoadAllFeaturedCommand = new AsyncRelayCommand(LoadAllFeaturedAsync, () => !HasNoPackageSources);
        }

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly FluentStoreApiClient FSApi = Ioc.Default.GetRequiredService<FluentStoreApiClient>();

        private void CarouselItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowCarousel = CarouselItems.Count > 0;

            if (ShowCarousel && SelectedCarouselItemIndex < 0)
                SelectedCarouselItemIndex = 0;
        }

        public async Task LoadAllFeaturedAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            try
            {
                await Task.WhenAll(LoadCarouselAsync(), LoadFeaturedAsync());

                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            }
            catch (System.Exception ex)
            {
                var logger = Ioc.Default.GetRequiredService<LoggerService>();
                logger.UnhandledException(ex, Microsoft.Extensions.Logging.LogLevel.Warning);
            }
        }

        public async Task LoadFeaturedAsync()
        {
            FeaturedPackages.Clear();

            // Load featured packages from other sources
            await foreach (HandlerPackageListPair pair in PackageService.GetFeaturedPackagesAsync())
                FeaturedPackages.Add(pair);
        }

        public async Task LoadCarouselAsync()
        {
            var featured = await FSApi.GetHomePageFeaturedAsync(Windows.ApplicationModel.Package.Current.Id.Version.ToVersion());
            CarouselItems.CollectionChanged += CarouselItems_CollectionChanged;
            CarouselItems.Clear();

            await featured.Carousel.InParallel(async item =>
            {
                try
                {
                    Urn packageUrn = Urn.Parse(item.PackageUrn);
                    var package = await PackageService.GetPackageAsync(packageUrn);
                    CarouselItems.Add(new PackageViewModel(package));
                }
                catch (System.Exception ex)
                {
                    var logger = Ioc.Default.GetRequiredService<LoggerService>();
                    logger.Warn(ex, ex.Message);
                }
            });

            CarouselItems.CollectionChanged -= CarouselItems_CollectionChanged;
        }

        public bool HasNoPackageSources => !PackageService.GetEnabledPackageHandlers().Any();

        private IAsyncRelayCommand _LoadAllFeaturedCommand;
        public IAsyncRelayCommand LoadAllFeaturedCommand
        {
            get => _LoadAllFeaturedCommand;
            set => SetProperty(ref _LoadAllFeaturedCommand, value);
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

        private ObservableCollection<HandlerPackageListPair> _FeaturedPackages = new();
        public ObservableCollection<HandlerPackageListPair> FeaturedPackages
        {
            get => _FeaturedPackages;
            set => SetProperty(ref _FeaturedPackages, value);
        }
    }
}
