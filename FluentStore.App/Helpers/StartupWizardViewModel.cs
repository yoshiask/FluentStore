using FluentStore.SDK.Plugins.Sources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Helpers;

public class StartupWizardViewModel
{
    public List<PluginPackageBase> PluginsToInstall { get; set; }
}
