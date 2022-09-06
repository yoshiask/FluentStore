using CommunityToolkit.Diagnostics;
using Flurl;
using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.Http;
using OwlCore.Storage.SystemIO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.SDK.Downloads
{
    public static class AbstractStorageHelper
    {
        public static Ipfs.Http.IpfsClient DefaultIpfsClient { get; set; } = new("http://127.0.0.1:5001");

        public static Windows.Web.Http.HttpClient DefaultHttpClient { get; set; } = new();

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
        /// An implementation of <see cref="IFile"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the URL is not supported.
        /// </exception>
        public static async Task<IFile> GetFileFromUrl(string url, CancellationToken cancellationToken = default)
        {
            (string scheme, string path) = GetSchemeAndPath(url);

            return scheme switch
            {
                "ipfs" or
                "ipns" => await GetIpfsFileFromUrl(url, cancellationToken),

                "http" or
                "https" => new HttpFile(url, DefaultHttpClient),

                "file" => new SystemFile(path),

                _ => throw new ArgumentException($"The '{scheme}' URL scheme is not supported.")
            };
        }

        public static async Task<IpfsFile> GetIpfsFileFromUrl(string url, CancellationToken cancellationToken = default)
        {
            // See https://github.com/ipfs/in-web-browsers/blob/dc1ce8d7718140eb3ae17681b0effd2e815ef8a8/ADDRESSING.md
            // for URI specification
            (string scheme, string id) = GetSchemeAndPath(url);

            if (scheme == "ipns")
            {
                string path = await DefaultIpfsClient.Name.ResolveAsync(id, recursive: true, cancel: cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                id = System.IO.Path.GetFileName(path);
            }

            Ipfs.Cid cid = id;
            IpfsFile file = new(cid, DefaultIpfsClient);
            return file;
        }

        private static (string scheme, string path) GetSchemeAndPath(string url)
        {
            var parts = url.Split(':', 2);
            return (parts[0], parts[1].TrimStart('/'));
        }
    }
}
