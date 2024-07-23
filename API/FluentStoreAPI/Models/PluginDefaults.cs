using Google.Apis.Firestore.v1.Data;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models;

public class PluginDefaults
{
    public PluginDefaults() { }

    internal PluginDefaults(Document d)
    {
        Feeds = d.Fields[nameof(Feeds)].ArrayValue.Values
            .Select(v => v.StringValue).ToList();
        Packages = d.Fields[nameof(Packages)].ArrayValue.Values
            .Select(v => v.StringValue).ToList();
    }

    public List<string> Packages { get; set; } = [];
    public List<string> Feeds { get; set; } = [];
}