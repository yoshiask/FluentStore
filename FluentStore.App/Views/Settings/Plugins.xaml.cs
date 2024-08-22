using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Plugins;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings
{
    public sealed partial class Plugins : UserControl
    {
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly PluginLoader PluginLoader = Ioc.Default.GetRequiredService<PluginLoader>();

        public Plugins()
        {
            this.InitializeComponent();
        }

        private void PackageHandlerEnable_Toggled(object sender, RoutedEventArgs e)
        {
            bool enable;
            if (sender is ToggleSwitch ts)
                enable = ts.IsOn;
            else if (sender is ToggleButton tb)
                enable = tb.IsChecked ?? false;
            else
                return;

            var handler = (sender as FrameworkElement)?.DataContext as PackageHandlerBase;
            if (handler is null)
                return;

            Helpers.Settings.Default.PackageHandlerEnabled[handler.Id] = enable;
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

        #region Install plugin
        private ContentDialog _pluginInstallDialog;
        private TextBlock _pluginInstallBox;

        private async void InstallPluginButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker openPicker = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
            };

            // Initialize save picker for Win32
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.Current.Window.Handle);

            openPicker.FileTypeFilter.Add(".nupkg");

            var pluginFile = await openPicker.PickSingleFileAsync();
            if (pluginFile != null)
            {
                WeakReferenceMessenger.Default.Register<ErrorMessage>(this, InstallPluginErrorMessage_Recieved);
                WeakReferenceMessenger.Default.Register<WarningMessage>(this, InstallPluginWarningMessage_Recieved);
                WeakReferenceMessenger.Default.Register<SuccessMessage>(this, InstallPluginSuccessMessage_Recieved);

                ProgressBar progressBar = new()
                {
                    IsIndeterminate = true,
                };
                _pluginInstallBox = new TextBlock
                {
                    Text = "Installing plugin, please wait...",
                    TextWrapping = TextWrapping.Wrap,
                    IsTextSelectionEnabled = true,
                };
                _pluginInstallDialog = new ContentDialog
                {
                    Title = "Plugin Manager",
                    Content = new ScrollViewer
                    {
                        Content = new StackPanel
                        {
                            Children =
                            {
                                progressBar,
                                _pluginInstallBox,
                            }
                        }
                    },
                    PrimaryButtonText = "OK",
                    IsPrimaryButtonEnabled = false,
                    IsSecondaryButtonEnabled = false,
                    XamlRoot = this.XamlRoot,
                };
                _pluginInstallDialog.ShowAsync();

                using var plugin = await pluginFile.OpenReadAsync();
                await PluginLoader.InstallPlugin(plugin.AsStream(), true);

                _pluginInstallDialog.IsPrimaryButtonEnabled = true;
                progressBar.Visibility = Visibility.Collapsed;

                WeakReferenceMessenger.Default.Unregister<ErrorMessage>(this);
                WeakReferenceMessenger.Default.Unregister<WarningMessage>(this);
                WeakReferenceMessenger.Default.Unregister<SuccessMessage>(this);
            }
        }

        private void InstallPluginErrorMessage_Recieved(object recipient, ErrorMessage message)
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
                _pluginInstallBox.Text = logMessage;
            });
        }

        private void InstallPluginSuccessMessage_Recieved(object recipient, SuccessMessage message)
        {
            if (message.Type is not (SuccessType.PluginDownloadCompleted or SuccessType.PluginInstallCompleted))
                return;

            _ = DispatcherQueue.TryEnqueue(delegate
            {
                _pluginInstallBox.Text = message.Message;
            });
        }

        private void InstallPluginWarningMessage_Recieved(object recipient, WarningMessage message)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                _pluginInstallBox.Text = message.Message;
            });
        }
        #endregion
    }
}
