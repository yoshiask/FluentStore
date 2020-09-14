using FluentStore.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MicrosoftStore.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentStore.ViewModels
{
    public class ProductDetailsViewModel : ObservableRecipient
    {
        private ObservableProductDetails product;
        public ObservableProductDetails Product
        {
            get => product;
            set => Set(ref product, value);
        }

        public BitmapImage GetAppIcon()
        {
            string url = "https://cdn.wallpaperhub.app/cloudcache/b/f/7/d/d/b/bf7ddbfb925701167ce8060cac808f88c641a16a.jpg";
            int width = 0;
            foreach (ImageItem image in Product.Images.FindAll(i => i.ImageType == "logo"))
            {
                if (image.Width > width)
                    url = image.Url;
            }
            return new BitmapImage(new System.Uri(url));
        }

        public BitmapImage GetHeroImage()
        {
            string url = "";
            int width = 0;
            foreach (ImageItem image in Product.Images.FindAll(i => i.ImageType == "hero"))
            {
                if (image.Width > width)
                    url = image.Url;
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                return new BitmapImage();
            }

            return new BitmapImage(new System.Uri(url));
        }

        public List<ImageItem> GetScreenshots()
        {
            return Product.Images.FindAll(i => i.ImageType == "screenshot");
        }

        public string GetAverageRatingString()
        {
            return Product.AverageRating.ToString("F1");
        }
    }
}
