using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using FluentStore.SDK.Plugins;
using OwlCore.Extensions;
using FluentStore.Services;
using System;

namespace FluentStore.Helpers;

public static class Extensions
{
    public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
    {
        int place = Source.LastIndexOf(Find);

        if (place == -1)
            return Source;

        string result = Source.Remove(place, Find.Length).Insert(place, Replace);
        return result;
    }

    public static Visibility HideIfNull(this object obj) => obj is null ? Visibility.Collapsed : Visibility.Visible;

    public static async Task InstallDefaultPlugins(this PluginLoader pluginLoader, bool overwrite = false)
    {
        try
        {
            var fsApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();
            var defaults = await fsApi.GetPluginDefaultsAsync();

            var defaultRepo = pluginLoader.Project.Repositories.Pop();
            pluginLoader.Project.Repositories.Clear();
            pluginLoader.Project.Repositories.Add(defaultRepo);
            pluginLoader.Project.AddFeeds(defaults.Feeds);

            await pluginLoader.InstallPlugins(defaults.Packages, overwrite);
        }
        catch (Exception ex)
        {
            var log = Ioc.Default.GetService<LoggerService>();
            log?.Warn(ex, "Failed to load default plugins");
        }
    }
}
