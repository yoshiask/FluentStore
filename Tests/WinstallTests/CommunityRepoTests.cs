using Winstall;

namespace WinstallTests;

public class CommunityRepoTests
{
    [Fact]
    public async Task GetVersionManifest()
    {
        const string id = "GeoGebra.Classic";
        const string version = "6.0.713";

        var manifest = await CommunityRepo.GetManifestAsync(id, version);

        Assert.NotNull(manifest);
        Assert.Equal(id, manifest.PackageIdentifier);
        Assert.Equal(version, manifest.PackageVersion);
    }

    [Fact]
    public async Task GetInstallerManifest()
    {
        const string id = "GeoGebra.Classic";
        const string version = "6.0.713";

        var manifest = await CommunityRepo.GetInstallerAsync(id, version);

        Assert.NotNull(manifest);
        Assert.Equal(id, manifest.PackageIdentifier);
        Assert.Equal(version, manifest.PackageVersion);

        Assert.NotNull(manifest.Installers);
        Assert.NotEmpty(manifest.Installers);

        var installer = manifest.Installers[0];
        Assert.NotNull(installer);
        Assert.False(string.IsNullOrEmpty(installer.InstallerUrl));
    }
}
