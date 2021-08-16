using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class VideoItem
    {
        [JsonProperty("VideoPurpose")]
        public VideoType VideoType { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string AudioEncoding { get; set; }
        public string VideoEncoding { get; set; }
        public V2.ImageItem Image { get; set; }
        public int Bitrate { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string VideoPositionInfo { get; set; }
        public int SortOrder { get; set; }

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
