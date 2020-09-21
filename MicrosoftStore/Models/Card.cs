using MicrosoftStore.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftStore.Models
{
    public class Card
    {
        public string ProductId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TileLayout TileLayout { get; set; }
        public string Title { get; set; }
        public List<ImageItem> Images { get; set; }
        public string DisplayPrice { get; set; }
        public double Price { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<string> PackageFamilyNames { get; set; }
        public List<string> ContentIds { get; set; }
        public bool GamingOptionsXboxLive { get; set; }
        public string AvailableDevicesDisplayText { get; set; }
        public string AvailableDevicesNarratorText { get; set; }
        public string TypeTag { get; set; }
        public string RecommendationReason { get; set; }
        public string LongDescription { get; set; }
        public string ProductFamilyName { get; set; }
        public string Schema { get; set; }
    }
}
