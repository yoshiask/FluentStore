using FluentStoreAPI.Models.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class DefaultPlugins
    {
        public DefaultPlugins(Document document)
        {
            PluginLists = new(document.Fields.Count);

            foreach (var fieldName in document.Fields.Keys)
            {
                var field = document.Fields[fieldName];
                var version = Version.Parse(fieldName);
                var plugins = ((IEnumerable<object>)document.TransformField(field)).OfType<string>();
                PluginLists.Add(version, plugins);
            }
        }

        Dictionary<Version, IEnumerable<string>> PluginLists { get; }

        public IEnumerable<string> GetDefaultPluginsForVersion(Version appVersion)
        {
            var curList = PluginLists.First();
            foreach (var list in PluginLists)
            {
                if (list.Key > appVersion)
                    continue;

                if (list.Key > curList.Key)
                    curList = list;
            }
            return curList.Value;
        }
    }
}
