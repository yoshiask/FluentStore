using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Plugins.Sources;
using FluentStore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace FluentStore.Controls;

public sealed partial class PackageManagerControl : UserControl,
    IRecipient<ErrorMessage>, IRecipient<WarningMessage>, IRecipient<SuccessMessage>,
    IRecipient<PluginDownloadProgressMessage>, IRecipient<PluginInstallStartedMessage>, IRecipient<PluginUninstallStartedMessage>
{
    private ContentDialog _pluginInstallDialog;
    private TextBlock _pluginInstallBox;
    private ProgressBar _pluginProgressBar;

    public PackageManagerControl()
    {
        this.InitializeComponent();

        if (ViewModel?.InstallCommand is not null)
            ViewModel.InstallCommand.PropertyChanged += PluginCommand_PropertyChanged;

        if (ViewModel?.UninstallCommand is not null)
            ViewModel.UninstallCommand.PropertyChanged += PluginCommand_PropertyChanged;
    }

    public PackageManagerViewModel ViewModel { get; } = new();

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(PackageManagerControl), new PropertyMetadata(null));

    private void PluginCommand_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IAsyncRelayCommand.IsRunning) || sender is not IAsyncRelayCommand command)
            return;

        if (command.IsRunning)
        {
            WeakReferenceMessenger.Default.RegisterAll(this);

            _pluginProgressBar = new()
            {
                IsIndeterminate = true,
            };
            _pluginInstallBox = new TextBlock
            {
                Text = "Getting ready...",
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
            if (_pluginInstallDialog is not null)
                _pluginInstallDialog.IsPrimaryButtonEnabled = true;

            if(_pluginProgressBar is not null)
                _pluginProgressBar.Visibility = Visibility.Collapsed;

            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }

    public void Receive(ErrorMessage message)
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
            case ErrorType.PluginUninstallFailed:
                logMessage = "Error uninstalling ";
                break;
            default:
                return;
        }

        logMessage += $"{message.Context}:\r\n{message.Exception}";
        SetInstallBoxText(logMessage);
    }

    public void Receive(WarningMessage message) => SetInstallBoxText(message.Message);

    public void Receive(SuccessMessage message)
    {
        if (message.Type is not (SuccessType.PluginDownloadCompleted or SuccessType.PluginInstallCompleted or SuccessType.PluginUninstallCompleted))
            return;

        SetInstallBoxText(message.Message);
    }

    public void Receive(PluginDownloadProgressMessage message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _pluginInstallBox.Text = $"Downloading {message.PluginId}...";
            
            if (message.Total is not null or 0)
                _pluginProgressBar.Value = message.Downloaded / message.Total.Value;
        });
    }

    public void Receive(PluginInstallStartedMessage message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _pluginInstallBox.Text = $"Installing {message.PluginId}...";
            _pluginProgressBar.IsIndeterminate = true;
        });
    }

    public void Receive(PluginUninstallStartedMessage message)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _pluginInstallBox.Text = $"Uninstalling {message.PluginId}...";
            _pluginProgressBar.IsIndeterminate = true;
        });
    }

    private void SetInstallBoxText(string text) => DispatcherQueue.TryEnqueue(() => _pluginInstallBox.Text = text);
}
