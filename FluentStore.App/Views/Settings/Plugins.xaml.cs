using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Messages;
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
                await PluginLoader.InstallPlugin(Helpers.Settings.Default, plugin.AsStream(), true);
            }
        }

        #region Reinstall default plugins
        private async void ReinstallDefaultPluginsButton_Click(object sender, RoutedEventArgs e)
        {
            ReinstallDefaultPluginsButton.IsEnabled = false;
            DefaultPluginsLog.Items.Clear();
            WeakReferenceMessenger.Default.Register<ErrorMessage>(this, DefaultPluginErrorMessage_Recieved);
            WeakReferenceMessenger.Default.Register<SuccessMessage>(this, DefaultPluginSuccessMessage_Recieved);
            WeakReferenceMessenger.Default.Register<PluginDownloadProgressMessage>(this, DefaultPluginProgressMessage_Recieved);
            DefaultPluginProgressIndicator.Visibility = Visibility.Visible;
            DefaultPluginsSetting.IsExpanded = true;

            await Helpers.Settings.Default.InstallDefaultPlugins(false, true);

            WeakReferenceMessenger.Default.Unregister<ErrorMessage>(this);
            WeakReferenceMessenger.Default.Unregister<SuccessMessage>(this);
            WeakReferenceMessenger.Default.Unregister<PluginDownloadProgressMessage>(this);
            DefaultPluginProgressIndicator.Visibility = Visibility.Collapsed;
            DefaultPluginStatusBlock.Text = "Downloaded default plugins. Please restart Fluent Store.";
            ReinstallDefaultPluginsButton.IsEnabled = true;
        }

        private void DefaultPluginErrorMessage_Recieved(object recipient, ErrorMessage message)
        {
            string logMessage;
            switch (message.Type)
            {
                case ErrorType.PluginDownloadFailed:
                    logMessage = "Error downloading ";
                    break;
                case ErrorType.PluginInstallFailed:
                    logMessage = "Error installing ";
                    break;
                default:
                    return;
            }

            logMessage += $"{message.Context}:\r\n{message.Exception}";

            _ = DispatcherQueue.TryEnqueue(delegate
            {
                DefaultPluginsLog.Items.Add(logMessage);
            });
        }

        private void DefaultPluginSuccessMessage_Recieved(object recipient, SuccessMessage message)
        {
            if (message.Type != SuccessType.PluginDownloadCompleted || message.Type != SuccessType.PluginInstallCompleted)
                return;

            _ = DispatcherQueue.TryEnqueue(delegate
            {
                DefaultPluginProgressIndicator.ShowError = false;
                DefaultPluginProgressIndicator.IsIndeterminate = true;
                DefaultPluginsLog.Items.Add(message.Message);
            });
        }

        private void DefaultPluginProgressMessage_Recieved(object recipient, PluginDownloadProgressMessage message)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                DefaultPluginProgressIndicator.ShowError = false;
                if (message.Total is null || message.Total == 0)
                {
                    DefaultPluginProgressIndicator.IsIndeterminate = true;
                }
                else
                {
                    DefaultPluginProgressIndicator.IsIndeterminate = false;
                    DefaultPluginProgressIndicator.Value = message.Downloaded / (double)message.Total;
                }

                DefaultPluginStatusBlock.Text = $"Downloading {message.PluginId} plugin...";
            });
        }
        #endregion
    }
}
