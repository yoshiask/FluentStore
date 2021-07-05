using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V2
{
    public class ImageItem
    {
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

        public Uri GetSource()
        {
            return string.IsNullOrWhiteSpace(Url) ? new Uri("https://cdn.wallpaperhub.app/cloudcache/b/f/7/d/d/b/bf7ddbfb925701167ce8060cac808f88c641a16a.jpg") : new Uri(Url);
        }
    }
}
