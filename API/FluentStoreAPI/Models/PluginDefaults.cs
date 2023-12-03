using FluentStoreAPI.Models.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models;

public class DefaultPlugins
{
    public List<string> Packages { get; set; } = new();

    /// <summary>
    /// Used by <see cref="Document"/> for deserialization
    /// </summary>
    public void SetPackages(List<object> objItems)
    {
        Packages = objItems.Cast<string>().ToList();
    }

    public IEnumerable<string> GetDefaultPluginsForVersion(Version appVersion, string arch)
    {
        var curEntry = PluginLists.First();
        foreach (var entry in PluginLists)
        {
            if (entry.Arch == arch
                && entry.Version <= appVersion
                && entry.Version >= curEntry.Version)
                curEntry = entry;
        }
        return curEntry.Urls;
    }
}