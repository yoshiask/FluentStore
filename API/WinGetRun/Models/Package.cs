using System;

namespace WinGetRun.Models
{
    public class Package
    {
        public string Id { get; set; }
        public string[] Versions { get; set; }
        public ManifestSummary Latest { get; set; }
        public bool Featured { get; set; }
        public string? IconUrl { get; set; }
        public string? Banner { get; set; }
        public string? Logo { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public static (string PublisherId, string PackageId) GetPublisherAndPackageIds(string combinedId)
        {
            int split = combinedId.IndexOf('.');
            return (combinedId.Substring(0, split), combinedId.Substring(split + 1));
        }
        public (string PublisherId, string PackageId) GetPublisherAndPackageIds() => GetPublisherAndPackageIds(Id);
    }
}
