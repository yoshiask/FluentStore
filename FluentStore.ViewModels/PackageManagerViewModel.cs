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
    }

    [ObservableProperty]
    private PackageViewModel _packageToView;

    [ObservableProperty]
    private ObservableCollection<PackageViewModel> _selectedPackages = new();

    [ObservableProperty]
    private ObservableCollection<PackageViewModel> _packages = new();

    [ObservableProperty]
    private PackageHandlerBase _handler;

    [ObservableProperty]
    private IAsyncRelayCommand _loadPackagesCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _installCommand;

    [ObservableProperty]
    private IAsyncRelayCommand _uninstallCommand;

    [ObservableProperty]
    private bool _isManagerEnabled = true;

    public async Task LoadPackagesAsync(CancellationToken token = default)
    {
        WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

        try
        {
            Handler ??= ActivatorUtilities.CreateInstance<NuGetPluginHandler>(Ioc.Default);

            Packages.Clear();

            await foreach (var package in Handler.GetFeaturedPackagesAsync())
                Packages.Add(package);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage(ex));
        }

        WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
    }

    public async Task InstallAsync(CancellationToken token = default)
    {
        IsManagerEnabled = false;

        foreach (var pvm in SelectedPackages)
        {
            token.ThrowIfCancellationRequested();

            await pvm.Package.InstallAsync();
        }

        IsManagerEnabled = true;
    }

    public async Task UninstallAsync(CancellationToken token = default)
    {
        IsManagerEnabled = false;

        foreach (var pvm in SelectedPackages)
        {
            token.ThrowIfCancellationRequested();

            if (pvm.Package is PluginPackageBase plugin)
                await plugin.UninstallAsync();
        }

        IsManagerEnabled = true;
    }
}
