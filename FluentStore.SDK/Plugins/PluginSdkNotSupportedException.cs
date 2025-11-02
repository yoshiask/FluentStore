using FluentStore.SDK.Plugins.NuGet;
using NuGet.Versioning;
using System;

namespace FluentStore.SDK.Plugins;

public class PluginSdkNotSupportedException(string pluginId, VersionRange supportedSdkRange)
    : Exception($"{pluginId} does not support Fluent Store SDK {FluentStoreNuGetProject.CurrentSdkVersion}: requires {supportedSdkRange}")
{
    public VersionRange SupportedSdkRange { get; } = supportedSdkRange;
}
