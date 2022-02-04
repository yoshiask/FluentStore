using Chocolatey;
using Chocolatey.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ChocolateyTests
{
    public class GetPackageProperty
    {
        [Fact]
        public async Task GetPackagePropertyAsync()
        {
            string id = "git";
            Version v = new(2, 35, 1, 2);
            string actual;

            actual = await Choco.GetPackagePropertyAsync(id, v, "Title");
            Assert.Equal("Git", actual);

            actual = await Choco.GetPackagePropertyAsync(id, v, "Id");
            Assert.Equal(id, actual);

            actual = await Choco.GetPackagePropertyAsync(id, v, "GalleryDetailsUrl");
            Assert.Equal($"https://community.chocolatey.org/packages/{id}/{v}", actual);
        }

        [Fact]
        public async Task GetPackageDatePropertyAsync()
        {
            string id = "git";
            Version v = new(2, 35, 1, 2);
            DateTimeOffset actual;

            actual = await Choco.GetPackageDatePropertyAsync(id, v, "Created");
            Assert.Equal(DateTimeOffset.Parse("2022-02-01T18:09:34.013"), actual);

            actual = await Choco.GetPackageDatePropertyAsync(id, v, "Published");
            Assert.Equal(DateTimeOffset.Parse("2022-02-01T18:09:34.013"), actual);

            actual = await Choco.GetPackageDatePropertyAsync(id, v, "PackageReviewedDate");
            Assert.Equal(DateTimeOffset.Parse("2022-02-02T01:46:13.997"), actual);
        }

        [Fact]
        public async Task GetPackageBooleanPropertyAsync()
        {
            string id = "git";
            Version v = new(2, 35, 1, 2);
            bool actual;

            actual = await Choco.GetPackageBooleanPropertyAsync(id, v, "IsPrerelease");
            Assert.False(actual);

            actual = await Choco.GetPackageBooleanPropertyAsync(id, v, "IsApproved");
            Assert.True(actual);

            actual = await Choco.GetPackageBooleanPropertyAsync(id, v, "RequireLicenseAcceptance");
            Assert.False(actual);
        }

        [Fact]
        public async Task GetPackageInt32PropertyAsync()
        {
            string id = "git";
            Version v = new(2, 35, 1, 2);
            int actual;

            // Don't assert an exact value, download counts are subject to change
            actual = await Choco.GetPackageInt32PropertyAsync(id, v, "DownloadCount");

            actual = await Choco.GetPackageInt32PropertyAsync(id, v, "VersionDownloadCount");
        }

        [Fact]
        public async Task GetPackageInt64PropertyAsync()
        {
            string id = "git";
            Version v = new(2, 35, 1, 2);
            long actual;

            actual = await Choco.GetPackageInt64PropertyAsync(id, v, "PackageSize");
            Assert.Equal(8170L, actual);
        }

        [Fact]
        public async Task GetPackagePropertyAsync_Enum()
        {
            string id = "git";
            Version v = new(2, 35, 1, 2);
            PackageStatus actual;

            actual = await Choco.GetPackagePropertyAsync(id, v, "PackageStatus", s => Enum.Parse<PackageStatus>(s));
            Assert.Equal(PackageStatus.Approved, actual);
        }
    }
}
