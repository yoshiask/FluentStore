namespace SDKTests;

public class Firebase
{
    private const string USERID = "2UYoF8HWrNOyzRaGe4EWONiEL003";
    private const string EMAIL = "test@askharoun.com";
    private const string PASSWORD = "yol6xKcdkJ4WGD";

    [Fact]
    public async Task GetFeatured()
    {
        FluentStoreAPI.FluentStoreAPI api = new();

        var featured = await api.GetHomePageFeaturedAsync();

        Assert.NotNull(featured);
        Assert.NotEmpty(featured.Carousel);
    }

    [Fact]
    public async Task ListDefaultPlugins()
    {
        FluentStoreAPI.FluentStoreAPI api = new();

        var pluginDefaults = await api.GetPluginDefaultsAsync();

        Assert.NotNull(pluginDefaults);
        Assert.NotEmpty(pluginDefaults.Packages);
        Assert.NotEmpty(pluginDefaults.Feeds);
    }

    [Fact]
    public async Task ListPublicUserCollections()
    {
        var api = await CreateAuthenticated();

        var collections = await api.GetCollectionsAsync(USERID);

        Assert.NotNull(collections);
        Assert.NotEmpty(collections);
        Assert.All(collections, c =>
        {
            Assert.NotEqual(Guid.Empty, c.Id);
            Assert.Equal(USERID, c.AuthorId);
            Assert.True(c.IsPublic);
            Assert.NotEmpty(c.Items);
        });
    }

    private static async Task<FluentStoreAPI.FluentStoreAPI> CreateAuthenticated()
    {
        FluentStoreAPI.FluentStoreAPI api = new();
        await api.SignInAsync(EMAIL, PASSWORD);
        return api;
    }
}
