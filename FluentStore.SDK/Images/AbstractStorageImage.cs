using OwlCore.Storage;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.SDK.Images;

public class AbstractStorageImage(IFile file) : StreamImage()
{
    public IFile File { get; } = file;

    public override async Task<Stream> GetImageStreamAsync()
    {
        Stream = await File.OpenReadAsync();
        return Stream;
    }
}
