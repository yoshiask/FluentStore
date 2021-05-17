using FluentStore.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MicrosoftStore.Enums;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStore.ViewModels
{
    public class ProductDetailsViewModel : ObservableObject
    {
        public ProductDetailsViewModel()
        {
            ViewProductCommand = new RelayCommand<object>(ViewProduct);
        }
        public ProductDetailsViewModel(ProductDetails product)
        {
            ViewProductCommand = new RelayCommand<object>(ViewProduct);
            Product = product;
        }

        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        private ProductDetails product;
        public ProductDetails Product
        {
            get => product;
            set
            {
                SetProperty(ref product, value);

                // Reset cached properties
                AppIcon = null;
                HeroImage = null;
                Screenshots = null;
            }
        }

        private IRelayCommand<object> _ViewProductCommand;
        public IRelayCommand<object> ViewProductCommand
        {
            get => _ViewProductCommand;
            set => SetProperty(ref _ViewProductCommand, value);
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

        private IAsyncRelayCommand<object> _SaveToCollectionCommand;
        public IAsyncRelayCommand<object> SaveToCollectionCommand
        {
            get => _SaveToCollectionCommand;
            set => SetProperty(ref _SaveToCollectionCommand, value);
        }

        private ImageItem _AppIcon;
        public ImageItem AppIcon
        {
            get
            {
                if (_AppIcon == null)
                    AppIcon = Product?.Images
                        .FindAll(i => i.ImageType == ImageType.Logo || i.ImageType == ImageType.Tile)
                        .OrderByDescending(i => i.Height * i.Width).First();
                return _AppIcon;
            }
            set => SetProperty(ref _AppIcon, value);
        }

        private Uri _HeroImage;
        public Uri HeroImage
        {
            get
            {
                if (_HeroImage == null)
                {
                    string url = "";
                    int width = 0;
                    foreach (ImageItem image in Product?.Images.FindAll(i => i.ImageType == ImageType.Hero))
                    {
                        if (image.Width > width)
                            url = image.Url;
                    }
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        HeroImage = new Uri("https://via.placeholder.com/1");
                    }
                    else
                    {
                        HeroImage = new Uri(url);
                    }
                }

                return _HeroImage;
            }
            set => SetProperty(ref _HeroImage, value);
        }

        private List<ImageItem> _Screenshots;
        public List<ImageItem> Screenshots
        {
            get
            {
                if (_Screenshots == null)
                    Screenshots = Product?.Images.FindAll(i => i.ImageType == ImageType.Screenshot);

                return _Screenshots;
            }
            set => SetProperty(ref _Screenshots, value);
        }

        public string AverageRatingString => Product.AverageRating.ToString("F1");

        public bool SupportsPlatform(PlatWindows plat) => Product.AllowedPlatforms.Contains(plat);

        public void ViewProduct(object obj)
        {
            ProductDetails pd;
            switch (obj)
            {
                case ProductDetailsViewModel viewModel:
                    pd = viewModel.Product;
                    break;
                case ProductDetails product:
                    pd = product;
                    break;
                default:
                    throw new ArgumentException($"'{nameof(obj)}' is an invalid type: {obj.GetType().Name}");
            }
            NavigationService.Navigate("ProductDetailsView", pd);
        }
    }
}
