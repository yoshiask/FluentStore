namespace FluentStore.SDK.Messages;

public class PluginUninstallStartedMessage
{
    public PluginUninstallStartedMessage(string pluginId)
    {
        PluginId = pluginId;
    }

    public string PluginId { get; }
}
