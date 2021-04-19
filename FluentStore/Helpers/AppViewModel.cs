using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentStore.ViewModels
{
    public class AppViewModel : AppViewModelBase
    {
        private AppListEntry _Entry;
        public AppListEntry Entry
        {
            get => _Entry;
            set => SetProperty(ref _Entry, value);
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

        public async Task LoadIconSourceAsync()
        {
            try
            {
                var streamData = await Entry.DisplayInfo.GetLogo(new Windows.Foundation.Size(256, 256)).OpenReadAsync();
                var iconSource = new BitmapImage();
                iconSource.SetSource(streamData);
                IconSource = iconSource;
            }
            catch { }
        }

        public AppViewModel(AppListEntry entry, string packageFamily)
        {
            DisplayName = entry.DisplayInfo.DisplayName;
            Description = entry.DisplayInfo.Description;
            PackageFamilyName = packageFamily;
            Entry = entry;
            LaunchCommand = new AsyncRelayCommand(LaunchAsync);
            LoadIconSourceCommand = new AsyncRelayCommand(LoadIconSourceAsync);
        }

        public async Task<bool> LaunchAsync()
        {
            return await Entry.LaunchAsync();
        }
    }
}
