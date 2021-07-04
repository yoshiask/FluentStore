using FluentStore.SDK;
using FluentStore.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MicrosoftStore.Enums;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ImageType = FluentStore.SDK.ImageType;

namespace FluentStore.ViewModels
{
    public class PackageViewModel : ObservableObject
    {
        public PackageViewModel()
        {
            ViewProductCommand = new RelayCommand<object>(ViewPackage);
        }
        public PackageViewModel(PackageBase package)
        {
            ViewProductCommand = new RelayCommand<object>(ViewPackage);
            Package = package;
        }

        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        private PackageBase _Package;
        public PackageBase Package
        {
            get => _Package;
            set
            {
                SetProperty(ref _Package, value);

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

        private ImageBase _AppIcon;
        public ImageBase AppIcon
        {
            get
            {
                if (_AppIcon == null)
                    AppIcon = Package?.Images
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
                    foreach (ImageBase image in Package?.Images.FindAll(i => i.ImageType == ImageType.Hero))
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

        private List<ImageBase> _Screenshots;
        public List<ImageBase> Screenshots
        {
            get
            {
                if (_Screenshots == null)
                    Screenshots = Package?.Images.FindAll(i => i.ImageType == ImageType.Screenshot);

                return _Screenshots;
            }
            set => SetProperty(ref _Screenshots, value);
        }

        public string AverageRatingString => Package.AverageRating.HasValue
            ? Package.AverageRating.Value.ToString("F1")
            : string.Empty;

        //public bool SupportsPlatform(PlatWindows plat) => Package.AllowedPlatforms.Contains(plat);

        public void ViewPackage(object obj)
        {
            PackageBase pb;
            switch (obj)
            {
                case PackageViewModel viewModel:
                    pb = viewModel.Package;
                    break;
                case PackageBase package:
                    pb = package;
                    break;
                default:
                    throw new ArgumentException($"'{nameof(obj)}' is an invalid type: {obj.GetType().Name}");
            }
            NavigationService.Navigate("PackageView", pb);
        }
    }
}
