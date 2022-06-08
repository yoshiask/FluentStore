
namespace FluentStore.SDK.Messages
{
    public class PluginInstallStartedMessage
    {
        public PluginInstallStartedMessage(string pluginId)
        {
            PluginId = pluginId;
        }

        public string PluginId { get; }
    }
}
