using Flurl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Winstall.Models;

public class Pack
{
    public bool IsUnlisted { get; set; }

    /// <summary>
    /// The Winstall ID of this pack.
    /// </summary>
    [JsonProperty("_id")]
    public string Id { get; set; }

    /// <summary>
    /// The display title.
    /// </summary>
    public string Title { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    /// <summary>
    /// A list of apps in this pack.
    /// </summary>
    public IReadOnlyList<App> Apps { get; set; }

    /// <summary>
    /// The ID of the creator of this pack.
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    /// A background gradient.
    /// </summary>
    public string Accent { get; set; }

    /// <summary>
    /// When this pack was first created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When this pack was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    public override string ToString() => Title;
}