namespace SDKTests;

public class Firebase
{
    [Fact]
    public async Task ListDefaultPlugins()
    {
        FluentStoreAPI.FluentStoreAPI api = new();

        var pluginDefaults = await api.GetPluginDefaultsAsync();

        Assert.NotNull(pluginDefaults);
        Assert.NotEmpty(pluginDefaults.Packages);
        Assert.NotEmpty(pluginDefaults.Feeds);
    }
}
