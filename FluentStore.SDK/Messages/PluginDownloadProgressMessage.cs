namespace FluentStore.SDK.Messages
{
    public class PluginDownloadProgressMessage
    {
        public PluginDownloadProgressMessage(string pluginId, ulong downloaded, ulong? total)
        {
            PluginId = pluginId;
            Downloaded = downloaded;
            Total = total;
        }

        public string PluginId { get; }
        public ulong Downloaded { get; }
        public ulong? Total { get; }
    }
}
