using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluentStore.SDK.Images
{
    public class FileImage : StreamImage
    {
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

        public FileImage(string url) => Url = url;
        public FileImage(Uri uri) => Uri = uri;
        public FileImage() { }

        public override async Task<Stream> GetImageStreamAsync()
        {
            if (Stream == null)
            {
                if (Uri.IsFile)
                {
                    // Dangerous: Will throw on UWP if file isn't accessible
                    try
                    {
                        if (!File.Exists(Url))
                            Stream = null;
                        Stream = File.OpenRead(Url);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Stream = null;
                    }
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(Url);
                    if (!response.IsSuccessStatusCode)
                        Stream = null;

                    Stream = await response.Content.ReadAsStreamAsync();
                }
            }

            return Stream;
        }
    }
}
