using MicrosoftStore.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace MicrosoftStore.Models
{
    public class VideoItem
    {
        public string Title { get; set; }
        public string VideoPurpose { get; set; }
        public string Url { get; set; }
        public string AudioEncoding { get; set; }
        public string VideoEncoding { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public double Bitrate { get; set; }
        public string VideoPositionInfo { get; set; }
        public int SortOrder { get; set; }
        public ImageItem Image { get; set; }

        [JsonIgnore]
        public Uri Uri
        {
            get
            {
                try
                {
                    return new Uri(Url);
                }
                catch { return null; }
            }
        }
    }

    public class ImageItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ImageType ImageType { get; set; }
        public string Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Caption { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public string ImagePositionInfo { get; set; }

        [JsonIgnore]
        public Uri Uri
        {
            get
            {
                try
                {
                    return new Uri(Url);
                }
                catch { return null; }
            }
        }

        public BitmapImage GetSource()
        {
            return string.IsNullOrWhiteSpace(Url) ? new BitmapImage() : new BitmapImage(new Uri(Url));
        }
    }
}
