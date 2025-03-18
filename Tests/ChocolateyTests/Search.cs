using Chocolatey;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ChocolateyTests;

public class Search(ITestOutputHelper output)
{
    [Fact]
    public async Task SearchAsync_Minimum1()
    {
        var actualResults = await Choco.SearchAsync("git");
        foreach (var result in actualResults)
            output.WriteLine($"{result.Title} {result.Version}");

        Assert.Equal(30, actualResults.Count);
        
        var firstResult = actualResults[0];
        Assert.Equal("git", firstResult.Id);
        Assert.Equal("Git", firstResult.Title);
        Assert.StartsWith("https://community.chocolatey.org/api/v2/package/git/", firstResult.DownloadUrl);
    }

    [Fact]
    public async Task SearchAsync_MinimumWithSpace()
    {
        var actualResults = await Choco.SearchAsync("google chrome");
        foreach (var result in actualResults)
            output.WriteLine($"{result.Title} {result.Version}");

        Assert.Equal(30, actualResults.Count);
        foreach (var result in actualResults)
        {
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Title);
        }
    }

    [Fact]
    public async Task SearchAsync_Minimum2Pages()
    {
        string query = "python";
        int pageSize = 10;

        var results1 = await Choco.SearchAsync(query, top: pageSize, skip: 0);
        foreach (var result in results1)
            output.WriteLine($"{result.Title} {result.Version}");

        Assert.Equal(pageSize, results1.Count);
        foreach (var result in results1)
        {
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Title);
        }

        var results2 = await Choco.SearchAsync(query, top: pageSize, skip: pageSize);
        foreach (var result in results2)
            output.WriteLine($"{result.Title} {result.Version}");

        Assert.Equal(pageSize, results2.Count);
        foreach (var result in results2)
        {
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Title);
            Assert.DoesNotContain(results1, r1 => r1.Id == result.Id);
        }
    }
}
