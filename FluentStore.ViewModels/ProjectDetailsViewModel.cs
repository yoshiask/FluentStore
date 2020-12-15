using FluentStore.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;

namespace FluentStore.ViewModels
{
    public class ProductDetailsViewModel : ObservableObject
    {
        public ProductDetailsViewModel()
        {
            ViewProductCommand = new RelayCommand<object>(ViewProduct);
        }

        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        private ProductDetails product;
        public ProductDetails Product
        {
            get => product;
            set => SetProperty(ref product, value);
        }

        private IRelayCommand<object> _ViewProductCommand;
        public IRelayCommand<object> ViewProductCommand
        {
            get => _ViewProductCommand;
            set => SetProperty(ref _ViewProductCommand, value);
        }

        public Uri GetAppIcon()
        {
            string url = "https://via.placeholder.com/1";
            int width = 0;
            foreach (ImageItem image in Product.Images.FindAll(i => i.ImageType == MicrosoftStore.Enums.ImageType.Logo))
            {
                if (image.Width > width)
                    url = image.Url;
            }
            return new Uri(url);
        }

        public Uri GetHeroImage()
        {
            string url = "";
            int width = 0;
            foreach (ImageItem image in Product.Images.FindAll(i => i.ImageType == MicrosoftStore.Enums.ImageType.Hero))
            {
                if (image.Width > width)
                    url = image.Url;
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                return new Uri("https://via.placeholder.com/1");
            }

            return new Uri(url);
        }

        public List<ImageItem> GetScreenshots()
        {
            return Product.Images.FindAll(i => i.ImageType == MicrosoftStore.Enums.ImageType.Screenshot);
        }

        public string AverageRatingString => Product.AverageRating.ToString("F1");

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
            NavigationService.AppNavigate("ProductDetailsView", pd);
        }
    }
}
