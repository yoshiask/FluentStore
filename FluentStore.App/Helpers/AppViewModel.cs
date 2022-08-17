using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using Windows.Storage;

namespace FluentStore.ViewModels
{
    public class AppViewModel : AppViewModelBase
    {
        private readonly Windows.Foundation.Size ICON_SIZE = new(256, 256);

        private AppListEntry _Entry;
        public AppListEntry Entry
        {
            get => _Entry;
            set => SetProperty(ref _Entry, value);
        }

        private Package _Package;
        public Package Package
        {
            get => _Package;
            set => SetProperty(ref _Package, value);
        }

        private ImageSource _IconSource;
        public ImageSource IconSource
        {
            get => _IconSource;
            set => SetProperty(ref _IconSource, value);
        }

        private IAsyncRelayCommand _LoadIconSourceCommand;
        public IAsyncRelayCommand LoadIconSourceCommand
        {
            get => _LoadIconSourceCommand;
            set => SetProperty(ref _LoadIconSourceCommand, value);
        }

        public AppViewModel(Package package, AppListEntry entry, string packageFamily)
        {
            DisplayName = entry.DisplayInfo.DisplayName;
            Description = entry.DisplayInfo.Description;
            PackageFamilyName = packageFamily;
            Entry = entry;
            Package = package;
            LaunchCommand = new AsyncRelayCommand(LaunchFromEntryAsync);
            LoadIconSourceCommand = new AsyncRelayCommand(LoadIconSourceFromEntryAsync);
        }

        public AppViewModel(Package package)
        {
            DisplayName = package.DisplayName;
            Description = package.Description;
            Package = package;
            PackageFamilyName = package.Id.FamilyName;
            LaunchCommand = new AsyncRelayCommand(LaunchAsync);
            LoadIconSourceCommand = new AsyncRelayCommand(LoadIconSourceFromPackageAsync);
        }

        public async Task<bool> LaunchFromEntryAsync()
        {
            return await Entry.LaunchAsync();
        }

        public async Task LoadIconSourceFromEntryAsync()
        {
            try
            {
                var streamData = await Entry.DisplayInfo.GetLogo(ICON_SIZE).OpenReadAsync();
                var iconSource = new BitmapImage();
                iconSource.SetSource(streamData);
                IconSource = iconSource;
            }
            catch { }
        }

        public async Task LoadIconSourceFromPackageAsync()
        {
            try
            {
                IRandomAccessStream streamData;

                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
                {
                    streamData = await Package.GetLogoAsRandomAccessStreamReference(ICON_SIZE)
                        .OpenReadAsync();
                }
                else
                {
                    var logoFile = await StorageFile.GetFileFromPathAsync(Package.Logo.AbsolutePath);
                    streamData = await logoFile.OpenReadAsync();
                }

                var iconSource = new BitmapImage();
                iconSource.SetSource(streamData);
                IconSource = iconSource;
            }
            catch { }
        }
    }
}
