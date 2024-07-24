using FSAPI = FluentStoreAPI.FluentStoreAPI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using Garfoot.Utilities.FluentUrn;
using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.SDK.Models;
using FluentStore.SDK.Helpers;
using OwlCore.Extensions;
using System.Linq;

namespace FluentStore.ViewModels
{
    public class HomeViewModel : ObservableRecipient
    {
        public HomeViewModel()
        {
            LoadAllFeaturedCommand = new AsyncRelayCommand(LoadAllFeaturedAsync, () => !HasNoPackageSources);
        }

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();

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

            await featured.Carousel.InParallel(async id =>
            {
                try
                {
                    Urn packageUrn = Urn.Parse(id);
                    var package = await PackageService.GetPackageAsync(packageUrn);
                    CarouselItems.Add(new PackageViewModel(package));
                    //if (i == 0)
                    //    SelectedCarouselItemIndex = i;
                }
                catch (System.Exception ex)
                {
                    var logger = Ioc.Default.GetRequiredService<LoggerService>();
                    logger.Warn(ex, ex.Message);
                }
            });

            for (int i = 0; i < featured.Carousel.Count; i++)
            {
                
            }
            CarouselItems.CollectionChanged -= CarouselItems_CollectionChanged;
        }

        public bool HasNoPackageSources => !PackageService.PackageHandlers.Any(p => p.IsEnabled);

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
