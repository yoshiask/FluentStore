using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            set => SetProperty(ref _Url, value);
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

        public Uri Uri => new Uri(Url);

        public async Task<Stream> GetImageStreamAsync()
        {
            var uri = new Uri(Url);
            var client = new HttpClient();
            var response = await client.GetAsync(Url);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStreamAsync();
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageType
    {
        BoxArt,
        Logo,
        Poster,
        Tile,
        Hero,
        Screenshot,
        Trailer
    }
}
