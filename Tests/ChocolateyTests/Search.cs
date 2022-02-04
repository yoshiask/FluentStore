using Chocolatey;
using System.Threading.Tasks;
using Xunit;

namespace ChocolateyTests
{
    public class Search
    {
        [Fact]
        public async Task SearchAsync_Minimum1()
        {
            var actualResult = await Choco.SearchAsync("git");
            Assert.Equal(30, actualResult.Count);
            
            var firstResult = actualResult[0];
            Assert.Equal("git", firstResult.Id);
            Assert.Equal("Git", firstResult.Title);
            Assert.StartsWith("https://community.chocolatey.org/api/v2/package/git/", firstResult.DownloadUrl);
        }

        [Fact]
        public async Task SearchAsync_MinimumWithSpace()
        {
            var actualResult = await Choco.SearchAsync("google chrome");
            Assert.Equal(30, actualResult.Count);
            foreach (var result in actualResult)
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
            Assert.Equal(pageSize, results1.Count);
            foreach (var result in results1)
            {
                Assert.NotNull(result.Id);
                Assert.NotNull(result.Title);
            }

            var results2 = await Choco.SearchAsync(query, top: pageSize, skip: pageSize);
            Assert.Equal(pageSize, results2.Count);
            foreach (var result in results2)
            {
                Assert.NotNull(result.Id);
                Assert.NotNull(result.Title);
                Assert.DoesNotContain(results1, r1 => r1.Id == result.Id);
            }
        }
    }
}
