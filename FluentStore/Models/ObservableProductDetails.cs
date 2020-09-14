using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentStore.Models
{
    public class ObservableProductDetails : ObservableObject
    {
        private readonly ProductDetails details;

        public ObservableProductDetails(ProductDetails details) => this.details = details;

        public string Title
        {
            get => details.Title;
            set => Set(() => details.Title, value);
        }
        public string Description
        {
            get => details.Description;
            set => Set(() => details.Description, value);
        }
        public string PublisherName
        {
            get => details.PublisherName;
            set => Set(() => details.PublisherName, value);
        }
        public List<string> Notes
        {
            get => details.Notes;
            set => Set(() => details.Notes, value);
        }
        public string ProductId
        {
            get => details.ProductId;
            set => Set(() => details.ProductId, value);
        }
        public string MediaType
        {
            get => details.MediaType;
            set => Set(() => details.MediaType, value);
        }
        public double AverageRating
        {
            get => details.AverageRating;
            set => Set(() => details.AverageRating, value);
        }
        public ObservableCollection<string> AllowedPlatforms
        {
            get => new ObservableCollection<string>(details.AllowedPlatforms);
            set => Set(() => new ObservableCollection<string>(details.AllowedPlatforms), value);
        }
        public List<ImageItem> Images
        {
            get => details.Images;
            set => Set(() => details.Images, value);
        }
        public List<string> Features
        {
            get => details.Features;
            set => Set(() => details.Features, value);
        }

        public ProductDetails GetRaw()
        {
            return details;
        }
    }
}
