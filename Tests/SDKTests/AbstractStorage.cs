using FluentStore.SDK.Downloads;

namespace SDKTests;

public class AbstractStorage
{
    [Theory]
    [InlineData("ipfs://Qmf9AbwewsJCwrFfZh1Vy22aVFTaUdqFdYSckoGuJeKKgh")]
    [InlineData("ipfs://QmawceGscqN4o8Y8Fv26UUmB454kn2bnkXV5tEQYc4jBd6")]
    [InlineData("ipns://docs.ipfs.tech/how-to/address-ipfs-on-web/index.html#native-urls")]
    [InlineData("ipns://ipfs.askharoun.com/FluentStore/BetaInstaller/FluentStoreBeta.appinstaller")]
    public async Task Ipfs(string url)
    {
        var ipfsFile = AbstractStorageHelper.GetFileFromUrl(url);
        var stream = await ipfsFile.OpenStreamAsync();

        MemoryStream memStream = new();
        stream.CopyTo(memStream);
    }
}