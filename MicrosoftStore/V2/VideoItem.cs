using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V2
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
}
