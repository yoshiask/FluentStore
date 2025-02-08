using CommunityToolkit.Mvvm.ComponentModel;
using FluentStore.Helpers;
using FluentStore.SDK.Plugins.Sources;
using FluentStore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Oobe
{
    public sealed partial class InstallPlugins : WizardPageBase
    {
        private readonly ISettingsService _settings;

        private IReadOnlyList<_PluginWizardInstallInfo> Plugins { get; }

        public InstallPlugins(StartupWizardViewModel wizard, ISettingsService settings) : base(wizard)
        {
            _settings = settings;

            CanAdvance = false;

            Plugins = wizard.PluginsToInstall
                .Select(package => new _PluginWizardInstallInfo
                {
                    Package = package,
                })
                .ToList();

            this.InitializeComponent();
        }

        private async void InstallPluginsPage_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                plugin.Status = _PluginWizardStatus.Installing;

                var success = await plugin.Package.InstallAsync();

                plugin.Status = success
                    ? _PluginWizardStatus.Installed
                    : _PluginWizardStatus.Failed;
            }

            CanAdvance = true;
        }
    }

    public partial class _PluginWizardInstallInfo : ObservableObject
    {
        [ObservableProperty]
        private PluginPackageBase _package;

        [ObservableProperty]
        private _PluginWizardStatus _status = _PluginWizardStatus.Waiting;
    }

    public enum _PluginWizardStatus
    {
        Waiting,
        Installing,
        Installed,
        Failed
    }
}
