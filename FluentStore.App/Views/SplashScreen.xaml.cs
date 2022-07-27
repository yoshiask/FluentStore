using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Messages;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views
{
    public sealed partial class SplashScreen : UserControl
    {
        public SplashScreen()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Register<SuccessMessage>(this, SuccessMessage_Recieved);
            WeakReferenceMessenger.Default.Register<PluginDownloadProgressMessage>(this, ProgressMessage_Recieved);
            WeakReferenceMessenger.Default.Register<PluginInstallStartedMessage>(this, InstallStartedMessage_Recieved);

            Unloaded += (_, __) => WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        private void SuccessMessage_Recieved(object recipient, SuccessMessage message)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                ProgressIndicator.ShowError = false;
                ProgressIndicator.IsIndeterminate = true;
                StatusBlock.Text = message.Message;
            });
        }

        private void ProgressMessage_Recieved(object recipient, PluginDownloadProgressMessage message)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                ProgressIndicator.ShowError = false;
                if (message.Total is null || message.Total == 0)
                {
                    ProgressIndicator.IsIndeterminate = true;
                }
                else
                {
                    ProgressIndicator.IsIndeterminate = false;
                    ProgressIndicator.Value = 100 * message.Downloaded / (double)message.Total;
                }

                StatusBlock.Text = $"Downloading {message.PluginId} plugin...";
            });
        }

        private void InstallStartedMessage_Recieved(object recipient, PluginInstallStartedMessage message)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                ProgressIndicator.ShowError = false;
                ProgressIndicator.IsIndeterminate = true;
                StatusBlock.Text = $"Installing {message.PluginId} plugin...";
            });
        }
    }
}
