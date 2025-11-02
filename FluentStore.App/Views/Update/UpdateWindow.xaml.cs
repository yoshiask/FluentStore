using FluentStore.SDK;
using FluentStore.ViewModels;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinRT.Interop;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Update;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
internal sealed partial class UpdateWindow : WindowEx
{
    private bool _downloaded = false;

    public UpdateWindow(PackageBase package)
    {
        ViewModel = new(package);
        Title = package.PackageHandler.DisplayName;

        ExtendsContentIntoTitleBar = true;
        IsMaximizable = false;
        IsMinimizable = false;
        IsResizable = false;

        Width = 412;
        Height = 412;

        InitializeComponent();
    }

    public PackageViewModel ViewModel { get; }

    private async void AffirmativeButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Package.DownloadItem is null)
        {
            await DownloadAsync();
        }
        else
        {
            await InstallAsync();
        }
    }

    private void NegativeButton_Click(object sender, RoutedEventArgs e) => Close();

    private async Task DownloadAsync()
    {
        AffirmativeButton.IsEnabled = false;
        // TODO: Support cancellation
        NegativeButton.IsEnabled = false;

        // Ask user where to save installer
        Windows.Storage.Pickers.FolderPicker savePicker = new()
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
        };

        // Initialize save picker for Win32
        InitializeWithWindow.Initialize(savePicker, WindowNative.GetWindowHandle(this));

        var userFolder = await savePicker.PickSingleFolderAsync();
        if (userFolder is null)
        {
            Close();
            return;
        }

        // TODO: Show progress
        DownloadProgressBar.IsIndeterminate = true;
        DownloadProgressBar.Visibility = Visibility.Visible;

        HeaderBlock.Text = "Downloading update...";
        StatusBlock.Text = "Please do not close this window. You may continue use Fluent Store in the meantime.";
        StatusBlock.Visibility = Visibility.Visible;

        await ViewModel.Package.DownloadAsync(new(userFolder.Path));
        await Task.Delay(10000);

        _downloaded = true;

        DownloadProgressBar.Visibility = Visibility.Collapsed;
        HeaderBlock.Text = "Download succeeded. Would you like to install now?";
        StatusBlock.Text = "Fluent Store will need to close.";

        AffirmativeButton.Content = "Install";
        AffirmativeButton.IsEnabled = true;
        NegativeButton.Content = "Close";
        NegativeButton.IsEnabled = true;
    }

    private async Task InstallAsync()
    {
        // Create and run new process for installer
        ProcessStartInfo startInfo = new()
        {
            FileName = ViewModel.Package.DownloadItem.FullName,
            UseShellExecute = true,
        };
        Process.Start(startInfo);

        await Task.Delay(1000);

        App.Current.Exit();
    }
}
