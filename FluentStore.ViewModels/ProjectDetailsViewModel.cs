using Microsoft.Toolkit.Mvvm.ComponentModel;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;

namespace FluentStore.ViewModels
{
    public class ProductDetailsViewModel : ObservableObject
    {
        private ProductDetails product;
        public ProductDetails Product
        {
            get => product;
            set => SetProperty(ref product, value);
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

        public string GetAverageRatingString()
        {
            return Product.AverageRating.ToString("F1");
        }
    }
}
