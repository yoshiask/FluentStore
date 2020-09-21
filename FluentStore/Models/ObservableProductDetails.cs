using Microsoft.Toolkit.Mvvm.ComponentModel;
using MicrosoftStore.Models;
using System.Collections.Generic;

namespace FluentStore.Models
{
    public class ObservableProductDetails : ObservableObject
    {
        private readonly ProductDetails details;

        public ObservableProductDetails(ProductDetails details) => this.details = details;

        public string Title
        {
            get => details.Title;
            set => SetProperty(() => details.Title, value);
        }
        public string Description
        {
            get => details.Description;
            set => SetProperty(() => details.Description, value);
        }
        public string PublisherName
        {
            get => details.PublisherName;
            set => SetProperty(() => details.PublisherName, value);
        }
        public List<string> Notes
        {
            get => details.Notes;
            set => SetProperty(() => details.Notes, value);
        }
        public string ProductId
        {
            get => details.ProductId;
            set => SetProperty(() => details.ProductId, value);
        }
        public string MediaType
        {
            get => details.MediaType;
            set => SetProperty(() => details.MediaType, value);
        }
        public double AverageRating
        {
            get => details.AverageRating;
            set => SetProperty(() => details.AverageRating, value);
        }
        public List<string> AllowedPlatforms
        {
            get => details.AllowedPlatforms;
            set => SetProperty(() => details.AllowedPlatforms, value);
        }
        public List<ImageItem> Images
        {
            get => details.Images;
            set => SetProperty(() => details.Images, value);
        }
        public List<string> Features
        {
            get => details.Features;
            set => SetProperty(() => details.Features, value);
        }

        public ProductDetails GetRaw()
        {
            return details;
        }
    }
}
