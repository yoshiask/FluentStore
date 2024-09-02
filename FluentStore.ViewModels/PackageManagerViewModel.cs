using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Plugins.Sources;
using FluentStore.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.ViewModels;

public partial class PackageManagerViewModel : ObservableObject
{
    public PackageManagerViewModel()
    {
        LoadPackagesCommand = new AsyncRelayCommand(LoadPackagesAsync);
        InstallCommand = new AsyncRelayCommand(InstallAsync);
        UninstallCommand = new AsyncRelayCommand(UninstallAsync);
        ViewCommand = new AsyncRelayCommand<PluginPackageBase>(ViewAsync);
    }

    [ObservableProperty]
    private PluginPackageBase _packageToView;

    // TODO: Combine with PackageToView once PackageBase supports everything PluginPackageBase does.
    [ObservableProperty]
    private PackageViewModel _packageViewModel;

    [ObservableProperty]
    private ObservableCollection<PluginPackageBase> _selectedPackages = new();

    [ObservableProperty]
    private ObservableCollection<PluginPackageBase> _packages = new();

    [ObservableProperty]
    private PackageHandlerBase _handler;

    [ObservableProperty]
    private IAsyncRelayCommand _loadPackagesCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _installCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _uninstallCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _viewCommand;

    [ObservableProperty]
    private bool _isManagerEnabled = true;

    public async Task LoadPackagesAsync(CancellationToken token = default)
    {
        WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
        IsManagerEnabled = false;

        try
        {
            Handler ??= ActivatorUtilities.CreateInstance<NuGetPluginHandler>(Ioc.Default);

            Packages.Clear();

            await foreach (var package in Handler.GetFeaturedPackagesAsync().OfType<PluginPackageBase>())
                Packages.Add(package);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage(ex));
        }

        IsManagerEnabled = true;
        WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
    }

    public async Task InstallAsync(CancellationToken token = default)
    {
        IsManagerEnabled = false;

        foreach (var package in SelectedPackages)
        {
            token.ThrowIfCancellationRequested();
            await package.InstallAsync();
        }

        IsManagerEnabled = true;
    }

    public async Task UninstallAsync(CancellationToken token = default)
    {
        IsManagerEnabled = false;

        foreach (var plugin in SelectedPackages)
        {
            token.ThrowIfCancellationRequested();
            await plugin.UninstallAsync();
        }

        IsManagerEnabled = true;
    }

    public async Task ViewAsync(PluginPackageBase package, CancellationToken token = default)
    {
        Guard.IsNotNull(package);

        if (package.Status != PackageStatus.Details)
        {
            var index = Packages.IndexOf(package);

            var urn = package.Urn;
            PackageToView = await Handler.GetPackage(urn) as PluginPackageBase;
        }
        else
        {
            PackageToView = package;
        }

        PackageViewModel = new(PackageToView);
    }
}
