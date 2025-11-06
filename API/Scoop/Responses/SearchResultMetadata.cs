using Flurl;
using System;

namespace Scoop.Responses;

public class SearchResultMetadata
{
    public string Repository { get; set; }
    public bool OfficialRepository { get; set; }
    public int RepositoryStars { get; set; }
    public string FilePath { get; set; }
    public string AuthorName { get; set; }
    public DateTimeOffset Committed { get; set; }
    public string Sha { get; set; }

    public Url GetManifestUrl() => $"{Repository}/raw/master/{FilePath}";
}
