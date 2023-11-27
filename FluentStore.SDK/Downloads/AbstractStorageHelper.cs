using FluentStore.SDK.Helpers;
using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.Archive;
using OwlCore.Storage.SystemIO;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.SDK.Downloads;

public static class AbstractStorageHelper
{
    static Ipfs.Http.IpfsClient _ipfsClient;

    public static Ipfs.Http.IpfsClient IpfsClient
    {
        get => _ipfsClient ??= new();
        set => _ipfsClient = value;
    }

    /// <summary>
    /// Creates an <see cref="IFile"/> from the given URL.
    /// </summary>
    /// <param name="url">
    /// The URL to use.
    /// </param>
    /// <returns>
    /// An implementation of <see cref="IFile"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the URL is not supported.
    /// </exception>
    public static IFile GetFileFromUrl(string url)
    {
        if (!TryGetFileFromUrl(url, out var file))
            throw new ArgumentException($"No OwlCore.Storage implementation exists for the following URL: {url}");
        return file;
    }

    /// <summary>
    /// Creates an <see cref="IFile"/> from the given URL.
    /// </summary>
    /// <param name="url">
    /// The URL to use.
    /// </param>
    /// <param name="desiredFileName">
    /// The desired filename, useful for situations where the URL
    /// may not contain an appropriate name.
    /// </param>
    /// <returns>
    /// Wether an implementation of <see cref="IFile"/>
    /// could be created.
    /// </returns>
    public static bool TryGetFileFromUrl(string url, out IFile file)
    {
        (string scheme, string path) = GetSchemeAndPath(url);

        file = scheme switch
        {
            "ipfs" or
            "ipns" => GetIpfsFileFromUrl(url),

            "http" or
            "https" => new WindowsHttpFile(new Uri(url)),

            "file" => new SystemFile(path),

            _ => null
        };

        return file is not null;
    }

    public static IFile GetIpfsFileFromUrl(string url)
    {
        // See https://github.com/ipfs/in-web-browsers/blob/dc1ce8d7718140eb3ae17681b0effd2e815ef8a8/ADDRESSING.md
        // for URI specification
        (string scheme, string id) = GetSchemeAndPath(url);

        // Remove fragments
        int fragmentIdx = id.IndexOf('#');
        if (fragmentIdx > 0)
            id = id[0..fragmentIdx];

        if (scheme == "ipns")
            return new IpnsFile($"/{scheme}/{id}", IpfsClient);

        return new IpfsFile(id, IpfsClient);
    }

    public static async Task<ZipArchiveFolder> CreateArchiveFromFolder(IFolder folder, IFile? archiveFile = null)
    {
        // Create new archive in memory if a destination file wasn't provided
        Stream archiveStream = archiveFile is null
            ? new MemoryStream()
            : await archiveFile.OpenStreamAsync(FileAccess.ReadWrite);

        ZipArchive archive = new(archiveStream, ZipArchiveMode.Update);
        ZipArchiveFolder archiveFolder = new(archive, new($"{Guid.NewGuid()}", $"{folder.Name}.zip"));

        await archiveFolder.CreateRecursiveCopyOfAsync(folder, extractToRoot: true);
        archiveStream.Flush();

        return archiveFolder;
    }

    public static Task<Stream> SafeOpenStreamAsync(this IFile file, FileAccess mode = FileAccess.Read, CancellationToken token = default)
    {
        return Task.Run(() => file.OpenStreamAsync(mode, token), token);
    }

    private static (string scheme, string path) GetSchemeAndPath(string url)
    {
        var parts = url.Split(':', 2);
        return (parts[0], parts[1].TrimStart('/'));
    }
}
