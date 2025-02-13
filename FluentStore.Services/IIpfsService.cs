using Ipfs;
using Ipfs.Http;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Kubo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.Services;

public interface IIpfsService : IDisposable, INotifyPropertyChanged
{
    IpfsClient Client { get; }

    bool IsRunning { get; }

    Task ConnectOrBootstrapAsync(ISettingsService settings, ICommonPathManager paths, CancellationToken token = default);

    Task BootstrapAsync(ISettingsService settings, ICommonPathManager paths, CancellationToken token = default);

    Task TestAsync(CancellationToken token = default);

    Task StopAsync();
}

public class IpfsService : IIpfsService
{
    private const string NODE_ID_ASKHAROUNCOM = "12D3KooWLD34NS3SbD6WzeWu2MrS6BtqtqkryhNSVBCBjNcd5EfQ";
    private const string IPv4_ASKHAROUNCOM = "45.15.24.95";
    private const string IPv6_ASKHAROUNCOM = "2a02:4780:1:1::1:9b3b";

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

    public async Task ConnectOrBootstrapAsync(ISettingsService settings, ICommonPathManager paths, CancellationToken token = default)
    {
        var apiPort = settings.IpfsApiPort;
        var existingClient = await TryConnectAsync(apiPort, token);

        if (existingClient is null)
        {
            await BootstrapAsync(settings, paths, token);
        }
        else
        {
            Client = existingClient;
            IsRunning = true;
        }
    }

    public async Task BootstrapAsync(ISettingsService settings, ICommonPathManager paths, CancellationToken token = default)
    {
        var kuboDir = paths.GetAppDataDirectory().CreateSubdirectory("Kubo");
        var kuboRepoDir = kuboDir.CreateSubdirectory("repo");
        var kuboBinDir = kuboDir.CreateSubdirectory("bin");

        var lockFile = kuboRepoDir
            .EnumerateFiles()
            .FirstOrDefault(f => f.Name.Equals("repo.lock", StringComparison.OrdinalIgnoreCase));
        if (lockFile is not null)
        {
            // Attempt to remove the repo lock, just in case a previous
            // run didn't exit cleanly
            try
            {
                lockFile.Delete();
            }
            catch { }
        }

        await StopAsync();
        
        _bootstrapper = new KuboBootstrapper(kuboRepoDir.FullName)
        {
            RoutingMode         = settings.RehostOnIpfs ? DhtRoutingMode.Auto : DhtRoutingMode.AutoClient,
            GatewayUri          = GetLocalUri(settings.IpfsGatewayPort),
            ApiUri              = GetLocalUri(settings.IpfsApiPort),
            LaunchConflictMode  = BootstrapLaunchConflictMode.Relaunch,
            BinaryWorkingFolder = new(kuboBinDir),
        };

        await _bootstrapper.StartAsync(token);

        // Add peers that are known to reliably serve Fluent Store content
        foreach (var peer in GetKnownPeers())
            await _bootstrapper.Client.Bootstrap.AddAsync(peer, token);

        Client = _bootstrapper.Client;
        IsRunning = true;
    }

    public async Task TestAsync(CancellationToken token = default)
    {
        // Random file to check general connection
        var cid = Cid.Decode("QmZtmD2qt6fJot32nabSP3CUjicnypEBz7bHVDhPQt9aAy");
        IpfsFile testFile = new(cid, Client);
        using (var stream = await testFile.OpenStreamAsync(cancellationToken: token))
        {
            var buffer = new byte[32];
            var bytesRead =
#if NETCOREAPP2_1_OR_GREATER
                await stream.ReadAsync(buffer, token);
#else
                await stream.ReadAsync(buffer, 0, buffer.Length, token);
#endif

            var testString = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            if (testString != "version 1 of my text\n")
                throw new Exception("Connected to IPFS, but test file was invalid. Please check your IPFS configuration.");
        }

        // Fluent Store file to check availability
        IpnsFolder testFolder = new("/ipns/ipfs.askharoun.com", Client);
        try
        {
            await testFolder
                .GetItemsAsync(cancellationToken: token)
                .AnyAsync(token);
        }
        catch (Exception ex)
        {
            throw new Exception("Connected to IPFS, but was unable to reach ipfs.askharoun.com. Please notify the maintainers.", ex);
        }
    }

    public async Task StopAsync()
    {
        Dispose();

        try
        {
            if (Client is not null)
                await Client.ShutdownAsync();
            
            IsRunning = false;
        }
        catch { }
    }

    public void Dispose()
    {
        _bootstrapper?.Dispose();
        _bootstrapper = null;
    }

    public static IEnumerable<MultiAddress> GetKnownPeers()
    {
        return [
            $"/ip4/{IPv4_ASKHAROUNCOM}/tcp/4001/p2p/{NODE_ID_ASKHAROUNCOM}",
            $"/ip4/{IPv4_ASKHAROUNCOM}/udp/4001/quic/p2p/{NODE_ID_ASKHAROUNCOM}",
            $"/ip6/{IPv6_ASKHAROUNCOM}/tcp/4001/p2p/{NODE_ID_ASKHAROUNCOM}",
            $"/ip6/{IPv6_ASKHAROUNCOM}/udp/4001/quic/p2p/{NODE_ID_ASKHAROUNCOM}",
        ];
    }

    private static Uri GetLocalUri(int port) => new($"http://127.0.0.1:{port}");

    public static async Task<IpfsClient> TryConnectAsync(int port, CancellationToken token = default)
    {
        var client = new IpfsClient
        {
            ApiUri = GetLocalUri(port),
        };

        try
        {
            var peer = await client.IdAsync(cancel: token);
            return client;
        }
        catch
        {
            return null;
        }
    }
}

public static class IIpfsServiceExtensions
{
    public static async Task BootstrapAsync(this IIpfsService ipfsService, IServiceProvider services, CancellationToken token = default)
    {
        var settings = services.GetRequiredService<ISettingsService>();
        var paths = services.GetRequiredService<ICommonPathManager>();
        await ipfsService.BootstrapAsync(settings, paths, token);
    }

    public static async Task ConnectOrBootstrapAsync(this IIpfsService ipfsService, IServiceProvider services, CancellationToken token = default)
    {
        var settings = services.GetRequiredService<ISettingsService>();
        var paths = services.GetRequiredService<ICommonPathManager>();
        await ipfsService.ConnectOrBootstrapAsync(settings, paths, token);
    }
}
