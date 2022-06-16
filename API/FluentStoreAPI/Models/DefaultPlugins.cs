using FluentStoreAPI.Models.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class DefaultPlugins
    {
        internal DefaultPlugins(Document document)
        {
            PluginLists = new(document.Fields.Count);

            foreach (var fieldName in document.Fields.Keys)
            {
                var field = document.Fields[fieldName];

                var listInfo = fieldName.Split('-');
                var version = Version.Parse(listInfo[0]);
                string arch = listInfo[1];

                var plugins = ((IEnumerable<object>)document.TransformField(field))?.OfType<string>()
                    ?? Array.Empty<string>();
                PluginLists.Add((version, arch, plugins));
            }
        }

        List<(Version Version, string Arch, IEnumerable<string> Urls)> PluginLists { get; }

        public IEnumerable<string> GetDefaultPluginsForVersion(Version appVersion, string arch)
        {
            var curEntry = PluginLists.First();
            foreach (var entry in PluginLists)
            {
                if (entry.Arch == arch
                    && entry.Version <= appVersion
                    && entry.Version > curEntry.Version)
                    curEntry = entry;
            }
            return curEntry.Urls;
        }
    }
}
