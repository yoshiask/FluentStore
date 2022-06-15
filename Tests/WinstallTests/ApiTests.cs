using Winstall;

namespace WinstallTests;

public class ApiTests
{
    WinstallApi _api = new();

    [Fact]
    public async Task Update()
    {
        var index = await _api.GetIndexAsync();

        Assert.NotNull(index);
        Assert.NotNull(index.Apps);
        Assert.NotEmpty(index.Apps);
    }

    [Fact]
    public async Task GetPack()
    {
        var props = await _api.GetPackAsync("qO_m22F6k");

        Assert.NotNull(props);
        Assert.NotNull(props.Pack);
        Assert.NotNull(props.Creator);

        Assert.Equal("winstall", props.Creator.Name);

        Assert.False(string.IsNullOrEmpty(props.Creator.CreatedAtStr));
        Assert.NotEqual(default, props.Creator.CreatedAt);
    }

    [Fact]
    public async Task Search()
    {
        var results = await _api.SearchAppsAsync("Visual studio");

        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, a => a.Id == "Microsoft.VisualStudio.2022.Community-Preview");
    }

    [Fact]
    public async Task GetApp()
    {
        const string id = "File-New-Project.EarTrumpet";
        var result = await _api.GetAppAsync(id);

        Assert.NotNull(result);
        Assert.NotNull(result.App);
        Assert.Equal(id, result.App.Id);
        Assert.Equal("EarTrumpet", result.App.Name);
        Assert.Contains("eartrumpet.app", result.App.HomepageUrl);
        Assert.False(string.IsNullOrEmpty(result.App.Description));
    }
}