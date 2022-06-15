using Flurl;
using Newtonsoft.Json;

namespace Winstall.Models;

public class PopularApp
{
    /// <summary>
    /// The file name of this entry's image.
    /// </summary>
    [JsonProperty("img")]
    public string Image { get; set; }

    /// <summary>
    /// The name of this app.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The identifier of this app.
    /// </summary>
    [JsonProperty("_id")]
    public string Id { get; set; }

    public string Path { get; set; }

    public Url GetImagePng() => Constants.GetImagePng(Image);

    public Url GetImageWebp() => Constants.GetImageWebp(Image);
}