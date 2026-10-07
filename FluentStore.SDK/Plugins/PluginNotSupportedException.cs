using FluentStore.SDK.Plugins.NuGet;
using NuGet.Versioning;
using System;

namespace FluentStore.SDK.Plugins;

public class PluginNotSupportedException(string pluginId, string reason)
    : Exception($"{pluginId} does not support this version of Fluent Store" + (reason != null ? $": {reason}" : ""));

public class PluginSdkNotSupportedException(string pluginId, VersionRange supportedSdkRange)
    : PluginNotSupportedException(pluginId, $"requires Fluent Store SDK {supportedSdkRange}, currently running {FluentStoreNuGetProject.CurrentSdkVersion}")
{
    public VersionRange SupportedSdkRange { get; } = supportedSdkRange;
}
