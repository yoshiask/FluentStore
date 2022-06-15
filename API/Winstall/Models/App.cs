using Flurl;
using FuseSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Winstall.Models;

[DebuggerDisplay("{Name} ({Id})")]
public class App : IFuseable
{
    /// <summary>
    /// The identifier for this app.
    /// </summary>
    [JsonProperty("_id")]
    public string Id { get; set; }

    public string Path { get; set; }

    /// <summary>
    /// The name of the file containing the app icon.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// The display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the app.
    /// </summary>
    [JsonProperty("desc")]
    public string Description { get; set; }

    /// <summary>
    /// The app moniker. Sometimes null.
    /// </summary>
    public string Moniker { get; set; }

    /// <summary>
    /// The name of the app publisher.
    /// </summary>
    public string Publisher { get; set; }

    /// <summary>
    /// The type of license.
    /// </summary>
    public string License { get; set; }

    /// <summary>
    /// A link to the license. May be null or empty.
    /// </summary>
    public string? LicenseUrl { get; set; }

    /// <summary>
    /// A link to the app's homepage. May be null or empty.
    /// </summary>
    [JsonProperty("homepage")]
    public string? HomepageUrl { get; set; }

    /// <summary>
    /// The minumum Windows version required to run this app.
    /// </summary>
    [JsonProperty("minOS")]
    public Version MinOSVersion { get; set; }

    /// <summary>
    /// The latest version of this app.
    /// </summary>
    public string LatestVersion { get; set; }

    /// <summary>
    /// All available versions of this app.
    /// </summary>
    public List<AppVersion> Versions { get; set; }

    /// <summary>
    /// When the app was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// A list of tags.
    /// </summary>
    public List<string> Tags { get; set; }

    public string SelectedVersion { get; set; }

    public IEnumerable<FuseProperty> Properties
    {
        get
        {
            const double nameWeight = 0.7;
            double tagWeight = (1 - nameWeight) / Tags.Count;

            FuseProperty[] props = new FuseProperty[Tags.Count + 1];
            props[0] = new(Name, nameWeight);
            for (int i = 0; i < Tags.Count; i++)
                props[i + 1] = new FuseProperty(Tags[i], tagWeight);

            return props;
        }
    }

    public Url GetImagePng() => Constants.GetImagePng(Icon);

    public Url GetImageWebp() => Constants.GetImageWebp(Icon);

    public Url GetWebLink() => Constants.GetWinstallAppLink(Moniker);

    public override string ToString() => Name;
}