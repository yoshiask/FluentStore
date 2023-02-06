using CommunityToolkit.Diagnostics;
using Flurl;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace OwlCore.Storage
{
    public class WindowsHttpFile : IFile
    {
        private readonly HttpClient _client;

        public WindowsHttpFile(string url, HttpClient client = null) : this(new Uri(url), client)
        {
        }

        public WindowsHttpFile(Url url, HttpClient client = null) : this(url.ToUri(), client)
        {
        }

        public WindowsHttpFile(Uri uri, HttpClient client = null)
        {
            _client = client ?? new();

            Uri = uri;
            Path = Id = uri.ToString();
            Name = Url.Decode(Uri.Segments[^1], true);
        }

        public Uri Uri { get; }

        public string Path { get; }

        public string Id { get; }

        public string Name { get; set; }

        public async Task<Stream> OpenStreamAsync(FileAccess accessMode = FileAccess.Read, CancellationToken cancellationToken = default)
        {
            Guard.IsEqualTo((int)accessMode, (int)FileAccess.Read, nameof(accessMode));

            var response = await _client.GetAsync(Uri, HttpCompletionOption.ResponseHeadersRead);
            var buffer = await response.Content.ReadAsBufferAsync();
            return buffer.AsStream();
        }
    }
}
