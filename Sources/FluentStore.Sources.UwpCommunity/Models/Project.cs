using System.Collections.Generic;

namespace FluentStore.Sources.UwpCommunity.Models;

public class Project : Modified
{
    public int Id { get; set; }
    public string AppName { get; set; }
    public string Description { get; set; }
    public bool IsPrivate { get; set; }
    public string DownloadLink { get; set; }
    public string GithubLink { get; set; }
    public string ExternalLink { get; set; }
    public List<Collaborator> Collaborators { get; set; }
    public string Category { get; set; }
    public bool? AwaitingLaunchApproval { get; set; }
    public bool NeedsManualReview { get; set; }
    public List<string> Images { get; set; }
    public List<string> Features { get; set; }
    public List<Tag> Tags { get; set; }
    public string HeroImage { get; set; }
    public string AppIcon { get; set; }
    public string AccentColor { get; set; }
    public string LookingForRoles { get; set; }
}
