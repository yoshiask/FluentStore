using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.Downloads;
using FluentStore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using System;
using System.IO.Compression;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings
{
    public sealed partial class General : UserControl
    {
        private readonly ICommonPathManager pathManager = Ioc.Default.GetService<ICommonPathManager>();

        public General()
        {
            this.InitializeComponent();
        }

        private void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            if (pathManager == null) return;

            var cacheDir = pathManager.GetTempDirectory();
            DownloadCache cache = new(cacheDir, createIfDoesNotExist: false);
            cache.Clear();
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            await Helpers.Settings.Default.ClearSettings();
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker openPicker = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            // Initialize save picker for Win32
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.Current.Window.Handle);

            openPicker.FileTypeFilter.Add(".zip");

            var settingsFile = await openPicker.PickSingleFileAsync();
            if (settingsFile != null)
            {
                ZipFile.ExtractToDirectory(settingsFile.Path, ((IAddressableFolder)Helpers.Settings.Default.Folder).Path, true);

                await Helpers.Settings.Default.LoadAsync();
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileSavePicker savePicker = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            // Initialize save picker for Win32
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.Current.Window.Handle);

            savePicker.FileTypeChoices.Add("Compressed Archive", new[] { ".zip" });
            savePicker.DefaultFileExtension = ".zip";
            savePicker.SuggestedFileName = $"FluentStoreBeta_Settings_{DateTime.Now:s}";

            var settingsFile = await savePicker.PickSaveFileAsync();
            if (settingsFile != null)
            {
                await Helpers.Settings.Default.SaveAsync();

                await settingsFile.DeleteAsync();

                ZipFile.CreateFromDirectory(((IAddressableFolder)Helpers.Settings.Default.Folder).Path,
                    settingsFile.Path, CompressionLevel.Optimal, false);

                var dialog = App.Current.Window.CreateMessageDialog($"Successfully exported settings to '{settingsFile.Path}'.");
                await dialog.ShowAsync();
            }
        }

        private void CrashButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            throw new Exception("An unhandled exception was thrown. " +
                "The app should have crashed and pushed a notification " +
                "that allows the user to view and report the error.");
#endif
        }
    }
}
