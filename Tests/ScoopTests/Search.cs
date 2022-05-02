using ScoopAPI;
using System.Threading.Tasks;
using Xunit;

namespace ScoopTests
{
    public class Search
    {
        [Fact]
        public async Task SearchAsync1()
        {
            var response = await ScoopSearch.SearchAsync("sharex");
            var first = response.Results[0];

            Assert.Equal(6, response.ODataCount);
            Assert.Equal("sharex", first.Name);
            Assert.Equal("https://getsharex.com/", first.Homepage);
            Assert.Equal("https://github.com/ScoopInstaller/Extras", first.Metadata.Repository);

            // Make sure manifest exists in bucket
            var manifest = await ScoopSearch.GetManifestAsync(first.Metadata);
        }
    }
}