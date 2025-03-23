using Scoop;
using Xunit.Abstractions;

namespace ScoopTests;

public class Search(ITestOutputHelper output)
{
    [Fact]
    public async Task SearchAsync_Minimum1()
    {
        var response = await ScoopSearch.SearchAsync("vscode");
        var actualResults = response.Results;
        Assert.Equal(3, actualResults.Count);

        var firstResult = actualResults[0];
        Assert.Equal("8706b3f90529133e6d1450c4e363645a1b24d4cf", firstResult.Id);
        Assert.Equal("vscode", firstResult.Name);
        Assert.Equal("https://code.visualstudio.com/", firstResult.Homepage);

        Assert.NotNull(firstResult.Metadata);
        Assert.Equal("https://github.com/ScoopInstaller/Extras", firstResult.Metadata.Repository);
        Assert.Equal("bucket/vscode.json", firstResult.Metadata.FilePath);
    }
}