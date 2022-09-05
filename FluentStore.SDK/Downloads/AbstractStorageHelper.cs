using Flurl;
using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.Http;
using OwlCore.Storage.SystemIO;
using System;
using System.IO;

namespace FluentStore.SDK.Downloads
{
    public static class AbstractStorageHelper
    {
        public static Ipfs.Http.IpfsClient DefaultIpfsClient { get; set; } = new("http://127.0.0.1:5001");

        public static Windows.Web.Http.HttpClient DefaultHttpClient { get; set; } = new();

        /// <summary>
        /// Creates an <see cref="IFile"/> from the given <see cref="Url"/>.
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
        public static IFile GetFileFromUrl(Url url, string desiredFileName = null)
        {
            return url.Scheme switch
            {
                "ipfs" => GetIpfsFileFromUrl(url),

                "http" or
                "https" => new HttpFile(url, DefaultHttpClient),

                "file" => new SystemFile(url.Path),

                _ => throw new ArgumentException($"The '{url.Scheme}' URL scheme is not supported.")
            };
        }

        public static IpfsFile GetIpfsFileFromUrl(Url url)
        {
            IpfsFile file = new(url.PathSegments[0], DefaultIpfsClient);
            return file;
        }
    }
}
