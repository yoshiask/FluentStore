using FluentStoreAPI.Models.Firebase;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models;

public class PluginDefaults
{
    public List<string> Packages { get; set; } = new();
    public List<string> Feeds { get; set; } = new();

    /// <summary>
    /// Used by <see cref="Document"/> for deserialization
    /// </summary>
    public void SetPackages(List<object> objItems)
    {
        Packages = objItems.Cast<string>().ToList();
    }

    /// <summary>
    /// Used by <see cref="Document"/> for deserialization
    /// </summary>
    public void SetFeeds(List<object> objItems)
    {
        Feeds = objItems.Cast<string>().ToList();
    }
}