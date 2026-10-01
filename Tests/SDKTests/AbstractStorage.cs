using FluentStore.SDK.Downloads;

namespace SDKTests;

public class AbstractStorage(ITestOutputHelper output)
{
    [Theory]
    [InlineData("ipfs://QmawceGscqN4o8Y8Fv26UUmB454kn2bnkXV5tEQYc4jBd6", true)]
    [InlineData("ipns://fluentstore.askharoun.com/Plugins/NuGet/index.json", false)]
    [InlineData("ipns://docs.ipfs.tech/how-to/address-ipfs-on-web/index.html#native-urls", false)]
    [InlineData("ipns://fluentstore.askharoun.com/BetaInstaller/FluentStoreBeta.appinstaller", false)]
    [InlineData("ipns://en.wikipedia-on-ipfs.org/wiki/Coptic_language", false)]
    public async Task Ipfs(string url, bool isBinary)
    {
        var ipfsFile = AbstractStorageHelper.GetFileFromUrl(url);
        using var stream = await ipfsFile
            .OpenStreamAsync(FileAccess.Read, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(20), TestContext.Current.CancellationToken);

        if (!isBinary)
        {
            using StreamReader reader = new(stream);
            var text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            output.Write(text);
        }
        else
        {
            // Force stream to be read
            var buffer = new byte[8192];
            var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);
            output.WriteLine($"Read {bytesRead} bytes from {url}");
        }
    }
}