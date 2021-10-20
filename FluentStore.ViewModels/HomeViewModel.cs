using FSAPI = FluentStoreAPI.FluentStoreAPI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Messages;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts;
using Garfoot.Utilities.FluentUrn;
using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.SDK.Models;

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

        public async Task LoadFeaturedAsync()
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

                var page = await StorefrontApi.GetHomeRecommendations();

                var featured = await FSApi.GetHomePageFeaturedAsync();
                CarouselItems.Clear();

                for (int i = 0; i < featured.Carousel.Count; i++)
                {
                    Urn packageUrn = Urn.Parse(featured.Carousel[i]);
                    var package = await PackageService.GetPackageAsync(packageUrn);
                    CarouselItems.Add(new PackageViewModel(package));
                    if (i == 0)
                        SelectedCarouselItemIndex = i;
                }

#if DEBUG
                // Add fake MS Store package for Fluent Store
                MicrosoftStorePackage fakeFSPackage = new()
                {
                    Categories = { "Utilities & tools" },
                    Description = "A unifying frontend for Windows app stores and package managers.",
                    DeveloperName = "Joshua \"Yoshi\" Askharoun",
                    DisplayPrice = "Free",
                    Features =
                    {
                        "Download MS Store apps without installing them",
                        "Create collections of apps to sync across devices and batch install",
                        "Discover and install apps from multiple sources, including WinGet and the Microsoft Store"
                    },
                    Images =
                    {
                        new SDK.Images.MicrosoftStoreImage
                        {
                            Url = "https://github.com/yoshiask/FluentStore/blob/master/.community/Hero.png?raw=true",
                            ImageType = SDK.Images.ImageType.Hero
                        },
                        new SDK.Images.MicrosoftStoreImage
                        {
                            Url = "https://github.com/yoshiask/FluentStore/blob/master/FluentStore/Assets/Square310x310Logo.scale-200.png?raw=true",
                            ImageType = SDK.Images.ImageType.Logo
                        },
                        new SDK.Images.MicrosoftStoreImage
                        {
                            Url = "https://github.com/yoshiask/FluentStore/blob/master/.community/Screenshots/PackageView_MSStore.png?raw=true",
                            ImageType = SDK.Images.ImageType.Screenshot,
                            ImagePositionInfo = "Desktop/0"
                        },
                        new SDK.Images.MicrosoftStoreImage
                        {
                            Url = "https://github.com/yoshiask/FluentStore/blob/master/.community/Screenshots/PackageView_Collection.png?raw=true",
                            ImageType = SDK.Images.ImageType.Screenshot,
                            ImagePositionInfo = "Desktop/1"
                        },
                        new SDK.Images.MicrosoftStoreImage
                        {
                            Url = "https://github.com/yoshiask/FluentStore/blob/master/.community/Screenshots/SearchResultsView_VS.png?raw=true",
                            ImageType = SDK.Images.ImageType.Screenshot,
                            ImagePositionInfo = "Desktop/2"
                        },
                    },
                    PackageFamilyName = "52374YoshiAskharoun.FluentStore_bcem08bwhrc72",
                    PublisherDisplayName = "YoshiAsk",
                    ReleaseDate = new System.DateTimeOffset(new System.DateTime(2021, 9, 1, 13, 0, 0)),
                    StoreId = "123456789123",
                    Title = "Fluent Store",
                    Urn = Urn.Parse("urn:microsoft-store:123456789123"),
                    Website = "https://github.com/yoshiask/FluentStore",
                    Version = "0.1.1.0"
                };
                CarouselItems.Add(new PackageViewModel(fakeFSPackage));
#endif

                // Load featured packages from other sources
                var rawFeatured = await PackageService.GetFeaturedPackagesAsync();
                FeaturedPackages = new ObservableCollection<HandlerPackageListPair>(rawFeatured);
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                NavService.ShowHttpErrorPage(ex);
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
