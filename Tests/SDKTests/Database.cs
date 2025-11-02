using FluentStoreAPI;
using FluentStoreAPI.Models;
using Xunit.Abstractions;

namespace SDKTests;

public class Database(ITestOutputHelper output)
{
    private static readonly Guid USERID = new("67e556ca-026c-418a-83db-fb390a8534ef");
    private const string EMAIL = "test@askharoun.com";
    private const string PASSWORD = "yol6xKcdkJ4WGD";

    [Fact]
    public async Task GetFeatured()
    {
        var api = await CreateClient();

        var featured = await api.GetHomePageFeaturedAsync();

        Assert.NotNull(featured);
        Assert.NotEmpty(featured.Carousel);

        foreach (var item in featured.Carousel)
            output.WriteLine(item.ToString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CRUDUserCollectionAuthed(bool isPublic)
    {
        var api = await CreateAuthedClient();

        var accessorString = isPublic ? "public" : "private";
        Collection newCollection = new()
        {
            Name = "Test Collection",
            Description = $"This is a {accessorString} collection created by the Fluent Store tests.",
            IsPublic = isPublic,
            TileGlyph = "TC",
            ImageUrl = null,
            Items = ["urn:microsoft-store:9NGHP3DX8HDX", "urn:microsoft-store:9WZDNCRDXF41"],
        };

        // 1: Create
        var id = await api.UpdateCollectionAsync(newCollection);
        Assert.NotNull(id);

        // 2: Read
        var collection = await api.GetCollectionAsync(id.Value);

        Assert.NotNull(collection);
        Assert.Equal(newCollection.Name, collection.Name);
        Assert.Equal(newCollection.Description, collection.Description);
        Assert.Equal(newCollection.IsPublic, collection.IsPublic);
        Assert.Equal(newCollection.IsPrivate, collection.IsPrivate);
        Assert.Equal(newCollection.TileGlyph, collection.TileGlyph);
        Assert.Equal(newCollection.ImageUrl, collection.ImageUrl);
        Assert.NotNull(collection.Items);
        Assert.Equal(newCollection.Items.Count, collection.Items.Count);

        for (int i = 0; i < newCollection.Items.Count; i++)
        {
            var newItem = newCollection.Items[i];
            var item = collection.Items[i];
            Assert.Equal(newItem, item);
        }

        Assert.Equal(newCollection, collection);

        await Task.Delay(500);

        // 3: Update
        collection.Name = "Edited Collection";
        collection.Description = "This collection has been edited.";
        collection.Items.RemoveAt(0);
        collection.Items.Insert(0, "urn:Microsoft-store:9mwv79xlfqh7");
        collection.Items.Add("urn:microsoft-store:9NBLGGH35LRM");
        var updatedCollection = collection;
        await api.UpdateCollectionAsync(updatedCollection);

        collection = await api.GetCollectionAsync(updatedCollection.Id);
        Assert.Equal(updatedCollection, collection);

        await Task.Delay(500);

        // 4: Delete
        await api.DeleteCollectionAsync(collection.Id);
        await Assert.ThrowsAnyAsync<Exception>(async () => await api.GetCollectionAsync(collection.Id));
    }

    [Fact]
    public async Task ListUserCollectionsAuthed()
    {
        var api = await CreateAuthedClient();

        var collections = await api.GetCollectionsAsync(USERID);

        Assert.NotNull(collections);
        Assert.NotEmpty(collections);
        Assert.All(collections, c =>
        {
            Assert.NotEqual(Guid.Empty, c.Id);
            Assert.Equal(USERID, c.AuthorId);
            Assert.NotEmpty(c.Items);
        });

        foreach (var c in collections)
            output.WriteLine($"{c.Name} ({c.Id})");
    }

    [Fact]
    public async Task ListPublicUserCollections()
    {
        var api = await CreateClient();

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

        foreach (var c in collections)
            output.WriteLine($"{c.Name} ({c.Id})");
    }

    private static async Task<FluentStoreApiClient> CreateAuthedClient()
    {
        FluentStoreApiClient api = new();
        await api.SignInAsync(EMAIL, PASSWORD);
        return api;
    }

    private static async Task<FluentStoreApiClient> CreateClient()
    {
        FluentStoreApiClient api = new();
        await api.InitAsync();
        return api;
    }
}
