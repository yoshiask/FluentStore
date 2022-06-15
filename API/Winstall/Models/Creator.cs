using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Winstall.Models
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Creator
    {
        public string Name { get; set; }
        public JObject Status { get; set; }
        public string Description { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAtStr { get; set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt => DateTimeOffset.ParseExact(
            CreatedAtStr, "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);

        public string Location { get; set; }
        public bool? GeoEnabled { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }
        public int StatusesCount { get; set; }
        public int FollowersCount { get; set; }
        public int FriendsCount { get; set; }
        public bool? Following { get; set; }
        public bool Protected { get; set; }
        public bool Verified { get; set; }
        public JObject Entities { get; set; }
        public bool? Notifications { get; set; }
        public string ProfileImageUrlHttp { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool? FollowRequestSent { get; set; }
        public bool DefaultProfile { get; set; }
        public bool DefaultProfileImage { get; set; }
        public int? FavoritesCount { get; set; }
        public int? ListedCount { get; set; }
        public string ProfileSidebarFillColor { get; set; }
        public string ProfileSidebarBorderColor { get; set; }
        public bool ProfileBackgroundTile { get; set; }
        public string ProfileBackgroundColor { get; set; }
        public string ProfileBackgroundImageUrl { get; set; }
        public string ProfileBackgroundImageUrlHttps { get; set; }
        public string ProfileBannerURL { get; set; }
        public string ProfileTextColor { get; set; }
        public string ProfileLinkColor { get; set; }
        public bool ProfileUseBackgroundImage { get; set; }
        public bool? IsTranslator { get; set; }
        public int? UtcOffset { get; set; }
        public bool? ContributorsEnabled { get; set; }
        public string TimeZone { get; set; }
        public IEnumerable<string> WithheldInCountries { get; set; }
        public string WithheldScope { get; set; }

        public string ScreenName;

        public long Id { get; set; }
        public string IdStr { get; set; }
    }
}
