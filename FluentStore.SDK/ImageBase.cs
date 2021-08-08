using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public class ImageBase : ObservableObject
    {
        private ImageType _ImageType;
        public ImageType ImageType
        {
            get => _ImageType;
            set => SetProperty(ref _ImageType, value);
        }

        private string _Url;
        public string Url
        {
            get => _Url;
            set
            {
                SetProperty(ref _Url, value);
                try
                {
                    SetProperty(ref _Uri, new Uri(value));
                }
                catch { }
            }
        }

        private int _Height;
        public int Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
        }

        private int _Width;
        public int Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }

        private string _BackgroundColor;
        public string BackgroundColor
        {
            get => _BackgroundColor;
            set => SetProperty(ref _BackgroundColor, value);
        }

        private string _ForegroundColor;
        public string ForegroundColor
        {
            get => _ForegroundColor;
            set => SetProperty(ref _ForegroundColor, value);
        }

        private Stream _Stream;
        public Stream Stream
        {
            get => _Stream;
            set => SetProperty(ref _Stream, value);
        }

        private Uri _Uri;
        public Uri Uri
        {
            get => _Uri;
            set
            {
                SetProperty(ref _Uri, value);
                SetProperty(ref _Url, value.ToString());
            }
        }

        public async Task<Stream> GetImageStreamAsync()
        {
            if (Stream != null)
                return Stream;

            if (Uri.IsFile)
            {
                // Dangerous: Will throw on UWP if file isn't accessible
                try
                {
                    if (!File.Exists(Url))
                        return null;
                    return File.OpenRead(Url);
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }
            }
            else
            {
                var client = new HttpClient();
                var response = await client.GetAsync(Url);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadAsStreamAsync();
            }
        }
    }

    public enum ImageType
    {
        Unspecified,
        BoxArt,
        Logo,
        Poster,
        Tile,
        Hero,
        Screenshot,
        Trailer
    }
}
