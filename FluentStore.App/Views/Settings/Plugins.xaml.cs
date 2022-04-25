using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings
{
    public sealed partial class Plugins : UserControl
    {
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public Plugins()
        {
            this.InitializeComponent();
        }

        private void PackageHandlerEnable_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleSwitch ts) return;

            Helpers.Settings.Default.SetPackageHandlerEnabledState(ts.DataContext.GetType().Name, ts.IsOn);
        }

        private void OpenPluginDirButton_Click(object sender, RoutedEventArgs e)
        {
            // Add a trailing slash to ensure that Explorer opens the folder,
            // and not a file that might have the same name
            System.Diagnostics.Process.Start("explorer.exe", $"\"{Helpers.Settings.Default.PluginDirectory}{Path.DirectorySeparatorChar}\"");
        }

        private void ResetPluginDirButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.Settings.Default.PluginDirectory = null;
        }

        private async void InstallPluginButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker openPicker = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
            };

            // Initialize save picker for Win32
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.Current.Window.Handle);

            openPicker.FileTypeFilter.Add(".zip");

            var pluginFile = await openPicker.PickSingleFileAsync();
            if (pluginFile != null)
            {
                var plugin = await pluginFile.OpenReadAsync();
                string pluginId = Path.GetFileNameWithoutExtension(pluginFile.Name);
                await PluginLoader.InstallPlugin(Helpers.Settings.Default, plugin.AsStream(), pluginId, true);
            }
        }
    }
}
