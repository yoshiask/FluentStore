using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Plugins.Sources;
using FluentStore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace FluentStore.Controls;

public sealed partial class PackageManagerControl : UserControl
{
    private ContentDialog _pluginInstallDialog;
    private TextBlock _pluginInstallBox;
    private ProgressBar _pluginProgressBar;

    public PackageManagerControl()
    {
        this.InitializeComponent();

        if (ViewModel?.InstallCommand is not null)
            ViewModel.InstallCommand.PropertyChanged += InstallCommand_PropertyChanged;
    }

    public PackageManagerViewModel ViewModel { get; } = new();

    private void PackageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var package in e.RemovedItems.Cast<PluginPackageBase>())
            ViewModel.SelectedPackages.Remove(package);

        foreach (var package in e.AddedItems.Cast<PluginPackageBase>())
            ViewModel.SelectedPackages.Add(package);
    }

    private async void PackageListView_ItemClick(object sender, ItemClickEventArgs e) =>
        await ViewModel.ViewCommand.ExecuteAsync(e.ClickedItem as PluginPackageBase);

    private void InstallCommand_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IAsyncRelayCommand.IsRunning) || sender is not IAsyncRelayCommand command)
            return;

        if (command.IsRunning)
        {
            WeakReferenceMessenger.Default.Register<ErrorMessage>(this, InstallPluginErrorMessage_Recieved);
            WeakReferenceMessenger.Default.Register<WarningMessage>(this, InstallPluginWarningMessage_Recieved);
            WeakReferenceMessenger.Default.Register<SuccessMessage>(this, InstallPluginSuccessMessage_Recieved);

            _pluginProgressBar = new()
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
                            _pluginProgressBar,
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
        }
        else
        {
            _pluginInstallDialog.IsPrimaryButtonEnabled = true;
            _pluginProgressBar.Visibility = Visibility.Collapsed;

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
}
