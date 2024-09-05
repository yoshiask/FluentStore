using Ipfs.Http;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Kubo;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.Services;

public interface IIpfsService : IDisposable, INotifyPropertyChanged
{
    IpfsClient Client { get; }

    bool IsRunning { get; }

    Task BootstrapAsync(ISettingsService settings, ICommonPathManager paths, CancellationToken token = default);

    void Stop();
}

public class IpfsService : IIpfsService
{
    private KuboBootstrapper _bootstrapper;
    private IpfsClient _client;
    private bool _isRunning;

    public IpfsClient Client
    {
        get => _client;
        set
        {
            _client = value;
            PropertyChanged?.Invoke(this, new(nameof(Client)));
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            PropertyChanged?.Invoke(this, new(nameof(IsRunning)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public async Task BootstrapAsync(ISettingsService settings, ICommonPathManager paths, CancellationToken token = default)
    {
        var kuboDir = paths.GetAppDataDirectory().CreateSubdirectory("Kubo");
        var kuboRepoDir = kuboDir.CreateSubdirectory("repo");
        var kuboBinDir = kuboDir.CreateSubdirectory("bin");

        Stop();
        
        _bootstrapper = new KuboBootstrapper(kuboRepoDir.FullName)
        {
            RoutingMode = settings.RehostOnIpfs ? DhtRoutingMode.Auto : DhtRoutingMode.AutoClient,
            GatewayUri = new($"http://127.0.0.1:{settings.IpfsGatewayPort}"),
            ApiUri = new($"http://127.0.0.1:{settings.IpfsApiPort}"),
            LaunchConflictMode = BootstrapLaunchConflictMode.Relaunch,
            BinaryWorkingFolder = new(kuboBinDir),
        };

        await _bootstrapper.StartAsync(token);

        Client = _bootstrapper.Client;
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
        _bootstrapper?.Dispose();
        _bootstrapper = null;
    }

    public void Dispose() => Stop();
}

public static class IIpfsServiceExtensions
{
    public static async Task BootstrapAsync(this IIpfsService ipfsService, IServiceProvider services, CancellationToken token = default)
    {
        var settings = services.GetRequiredService<ISettingsService>();
        var paths = services.GetRequiredService<ICommonPathManager>();
        await ipfsService.BootstrapAsync(settings, paths, token);
    }
}
