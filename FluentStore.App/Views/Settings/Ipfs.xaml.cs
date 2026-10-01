using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.Services;
using Humanizer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings;

public sealed partial class Ipfs : UserControl
{
    private readonly NavigationServiceBase _navigationService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
    private readonly ISettingsService _settings = Ioc.Default.GetRequiredService<ISettingsService>();
    private readonly IIpfsService _ipfs = Ioc.Default.GetRequiredService<IIpfsService>();

    public Ipfs()
    {
        this.InitializeComponent();
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateForRunning();

        if (_ipfs.IsRunning)
        {
            var peer = await _ipfs.Client.IdAsync();
            IdBlock.Text = peer.Id.ToString();

            var repoStats = await _ipfs.Client.Stats.RepositoryAsync();
            RepoPathBlock.Text = repoStats.RepoPath;
            RepoSizeBlock.Text = new ByteSize(repoStats.RepoSize).Humanize();
        }
    }

    private async void ViewDocsButton_Click(object sender, RoutedEventArgs e)
    {
        await _navigationService.OpenInBrowser("https://docs.ipfs.tech/concepts/what-is-ipfs/");
    }

    private async void StopNodeButton_Click(object sender, RoutedEventArgs e)
    {
        await _ipfs.StopAsync();
        UpdateForRunning();
    }

    private void UpdateForRunning()
    {
        if (!_ipfs.IsRunning)
        {
            StopNodeButton.IsEnabled = false;
            NodeStatusBlock.ExpandableContent = null;
        }
    }
}
