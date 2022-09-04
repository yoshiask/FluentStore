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
        private static readonly Ipfs.Http.IpfsClient DefaultLocalIpfsClient = new("http://127.0.0.1:5001");

        public static IFile GetFileFromUrl(Url url)
        {
            return url.Scheme switch
            {
                "ipfs" => GetIpfsFileFromUrl(url),

                "http" or
                "https" => new HttpFile(url),

                "file" => new SystemFile(url.Path),

                _ => throw new ArgumentException($"The '{url.Scheme}' URL scheme is not supported.")
            };
        }

        public static IpfsFile GetIpfsFileFromUrl(Url url)
        {
            IpfsFile file = new(url.PathSegments[0], DefaultLocalIpfsClient);
            return file;
        }
    }
}
