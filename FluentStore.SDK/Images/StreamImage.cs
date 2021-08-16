using FluentStore.SDK.Models;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.SDK.Images
{
    public class StreamImage : ImageBase
    {
        private Stream _Stream;
        public Stream Stream
        {
            get => _Stream;
            set => SetProperty(ref _Stream, value);
        }

        public virtual Task<Stream> GetImageStreamAsync()
        {
            return Task.FromResult(Stream);
        }
    }
}
