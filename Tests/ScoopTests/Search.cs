using Scoop;
using Xunit.Abstractions;

namespace ScoopTests;

public class Search(ITestOutputHelper output)
{
    public readonly ScoopSearch _search = new();

    [Fact]
    public async Task SearchAsync_Minimum1()
    {
        var response = await _search.SearchAsync("vscode");
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

    [Theory]
    [InlineData("vscode")]
    [InlineData("notepadplusplus")]
    [InlineData("notepadplusplus-np")]
    [InlineData("jetbrains-mono")]
    [InlineData("7zip")]
    [InlineData("7zip-beta")]
    public async Task GetManifestAsync_FromSearchResult(string name)
    {
        var response = await _search.SearchAsync(name);
        var searchResult = response.Results.FirstOrDefault();

        Assert.NotNull(searchResult);
        Assert.Equal(name, searchResult.Name, ignoreCase: true);
        Assert.NotNull(searchResult.Metadata);

        var manifest = await _search.GetManifestAsync(searchResult.Metadata);

        Assert.NotNull(manifest);

        output.WriteLine(manifest.Description ?? "{null}");
    }

    [Theory]
    [InlineData("stack", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/stack.json", "Add-Path -Path \"$env:APPDATA\\local\\bin\" -Global:$global")]
    [InlineData("cuda", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/cuda.json", "$names = @('bin', 'extras', 'include', 'lib', 'libnvvp', 'nvml', 'nvvm', 'compute-sanitizer')\r\nforeach ($name in $names) {\r\n    Copy-Item \"$dir\\cuda_*\\*\\$name\" \"$dir\" -Recurse -Force\r\n    Copy-Item \"$dir\\lib*\\*\\$name\" \"$dir\" -Recurse -Force\r\n}\r\nGet-ChildItem \"$dir\" -Exclude $names | Remove-Item -Recurse -Force")]
    [InlineData("go", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/go.json", "$envgopath = \"$env:USERPROFILE\\go\"\r\nif ($env:GOPATH) { $envgopath = $env:GOPATH }\r\ninfo \"Adding '$envgopath\\bin' to PATH...\"\r\nAdd-Path -Path \"$envgopath\\bin\" -Global:$global -Force")]
    public async Task DeserializeInstallerScript(string name, string manifestUrl, string fullScriptEx)
    {
        var manifest = await _search.GetManifestAsync(manifestUrl);

        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Installer);
        Assert.NotNull(manifest.Installer.Script);

        var fullScriptAc = string.Join("\r\n", manifest.Installer.Script);
        output.WriteLine(fullScriptAc);

        Assert.Equal(fullScriptEx, fullScriptAc);
    }

    [Theory]
    [InlineData("gpg", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/gpg.json", "GPL-3.0-or-later", null)]
    [InlineData("cuda", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/cuda.json", "Freeware", "https://docs.nvidia.com/cuda/eula/index.html")]
    [InlineData("xz", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/xz.json", "LGPL-2.1-or-later,GPL-2.0-or-later,GPL-3.0-or-later,Public Domain", "https://git.tukaani.org/?p=xz.git;a=blob;f=COPYING")]
    [InlineData("scc", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/scc.json", "MIT|Unlicense", null)]
    [InlineData("mongodb", "https://github.com/ScoopInstaller/Main/raw/489bc5445ae47a5504cb0fb1c32a43fa34d9a8f8/bucket/mongodb.json", "SSPL-1.0", "https://www.mongodb.com/licensing/server-side-public-license")]
    public async Task DeserializeAppLicense(string name, string manifestUrl, string identifierEx, string? urlEx)
    {
        var manifest = await _search.GetManifestAsync(manifestUrl);

        Assert.NotNull(manifest);
        Assert.NotNull(manifest.License);

        var identifierAc = string.Join("|", manifest.License.MultiLicenses.Select(d => string.Join(",", d)));
        Assert.Equal(identifierEx, identifierAc);

        var urlAc = manifest.License.Url?.ToString();
        Assert.Equal(urlEx, urlAc);
    }
}