#nullable disable
using System;

namespace Chocolatey.Models
{
    public class Package
    {
        public Package() { }
        
        /// <summary>
        /// Convert an Atom XML entry to a Chocolatey package.
        /// </summary>
        public Package(System.Xml.Linq.XElement entry)
        {
            // Atom properties
            Id = entry.Element(Constants.XMLNS_ATOM + "title").Value;
            Summary = entry.Element(Constants.XMLNS_ATOM + "summary").Value;
            Updated = DateTimeOffset.Parse(entry.Element(Constants.XMLNS_ATOM + "updated").Value);
            AuthorName = entry.Element(Constants.XMLNS_ATOM + "author")
                                  .Element(Constants.XMLNS_ATOM + "name").Value;
            DownloadUrl = entry.Element(Constants.XMLNS_ATOM + "content")
                                   .Attribute("src").Value;

            // Custom properties
            var props = entry.Element(Constants.XMLNS_ADO_DATASERVICES_METADATA + "properties");

            Version = Version.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "Version").Value);
            Title = props.Element(Constants.XMLNS_ADO_DATASERVICES + "Title").Value;
            Description = props.Element(Constants.XMLNS_ADO_DATASERVICES + "Description").Value;
            Tags = props.Element(Constants.XMLNS_ADO_DATASERVICES + "Tags").Value
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Copyright = props.Element(Constants.XMLNS_ADO_DATASERVICES + "Copyright").Value;
            Created = DateTimeOffset.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "Created").Value);
            Dependencies = props.Element(Constants.XMLNS_ADO_DATASERVICES + "Dependencies").Value;
            DownloadCount = int.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "DownloadCount").Value);
            VersionDownloadCount = int.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "VersionDownloadCount").Value);
            GalleryDetailsUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "GalleryDetailsUrl").Value;
            ReportAbuseUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "ReportAbuseUrl").Value;
            IconUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "IconUrl").Value;
            IsLatestVersion = bool.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "IsLatestVersion").Value);
            IsAbsoluteLatestVersion = bool.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "IsAbsoluteLatestVersion").Value);
            IsPrerelease = bool.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "IsPrerelease").Value);
            Language = props.Element(Constants.XMLNS_ADO_DATASERVICES + "Language").Value;
            Published = DateTimeOffset.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "Published").Value);
            LicenseUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "LicenseUrl").Value;
            RequireLicenseAcceptance = bool.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "RequireLicenseAcceptance").Value);
            PackageHash = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageHash").Value;
            PackageHashAlgorithm = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageHashAlgorithm").Value;
            PackageSize = long.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageSize").Value);
            ProjectUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "ProjectUrl").Value;
            ReleaseNotes = props.Element(Constants.XMLNS_ADO_DATASERVICES + "ReleaseNotes").Value;
            ProjectSourceUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "ProjectSourceUrl").Value;
            PackageSourceUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageSourceUrl").Value;
            DocsUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "DocsUrl").Value;
            MailingListUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "MailingListUrl").Value;
            BugTrackerUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "BugTrackerUrl").Value;
            IsApproved = bool.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "IsApproved").Value);
            PackageStatus = Extensions.TryParseEnum<PackageStatus>(props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageStatus").Value);
            PackageSubmittedStatus = Extensions.TryParseEnum<PackageStatus>(props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageSubmittedStatus").Value);
            PackageTestResultUrl = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageTestResultUrl").Value;
            PackageTestResultStatus = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageTestResultStatus").Value;
            PackageTestResultStatusDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageTestResultStatusDate").Value, out var ptrsd) ? ptrsd : null;
            PackageValidationResultStatus = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageValidationResultStatus").Value;
            PackageValidationResultDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageValidationResultDate").Value, out var pvrd) ? pvrd : null;
            PackageCleanupResultDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageCleanupResultDate").Value, out var pcrd) ? pcrd : null;
            PackageReviewedDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageReviewedDate").Value, out var prd) ? prd : null;
            PackageApprovedDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageApprovedDate").Value, out var pad) ? pad : null;
            PackageReviewer = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageReviewer").Value;
            IsDownloadCacheAvailable = bool.Parse(props.Element(Constants.XMLNS_ADO_DATASERVICES + "IsDownloadCacheAvailable").Value);
            DownloadCacheStatus = props.Element(Constants.XMLNS_ADO_DATASERVICES + "DownloadCacheStatus").Value;
            DownloadCacheDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "DownloadCacheDate").Value, out var dcd) ? dcd : null;
            DownloadCache = props.Element(Constants.XMLNS_ADO_DATASERVICES + "DownloadCache").Value;
            PackageScanStatus = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageScanStatus").Value;
            PackageScanResultDate = DateTimeOffset.TryParse(
                props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageScanResultDate").Value, out var psrd) ? psrd : null;
            PackageScanFlagResult = props.Element(Constants.XMLNS_ADO_DATASERVICES + "PackageScanFlagResult").Value;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string AuthorName { get; set; }
        public string DownloadUrl { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string Copyright { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Dependencies { get; set; }
        public int DownloadCount { get; set; }
        public int VersionDownloadCount { get; set; }
        public string GalleryDetailsUrl { get; set; }
        public string ReportAbuseUrl { get; set; }
        public string IconUrl { get; set; }
        public bool IsLatestVersion { get; set; }
        public bool IsAbsoluteLatestVersion { get; set; }
        public bool IsPrerelease { get; set; }
        public string Language { get; set; }
        public DateTimeOffset Published { get; set; }
        public string LicenseUrl { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string PackageHash { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public long PackageSize { get; set; }
        public string ProjectUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public string ProjectSourceUrl { get; set; }
        public string PackageSourceUrl { get; set; }
        public string DocsUrl { get; set; }
        public string MailingListUrl { get; set; }
        public string BugTrackerUrl { get; set; }
        public bool IsApproved { get; set; }
        public PackageStatus PackageStatus { get; set; }
        public PackageStatus PackageSubmittedStatus { get; set; }
        public string PackageTestResultUrl { get; set; }
        public string PackageTestResultStatus { get; set; }
        public DateTimeOffset? PackageTestResultStatusDate { get; set; }
        public string PackageValidationResultStatus { get; set; }
        public DateTimeOffset? PackageValidationResultDate { get; set; }
        public DateTimeOffset? PackageCleanupResultDate { get; set; }
        public DateTimeOffset? PackageReviewedDate { get; set; }
        public DateTimeOffset? PackageApprovedDate { get; set; }
        public string PackageReviewer { get; set; }
        public bool IsDownloadCacheAvailable { get; set; }
        public string DownloadCacheStatus { get; set; }
        public DateTimeOffset? DownloadCacheDate { get; set; }
        public string DownloadCache { get; set; }
        public string PackageScanStatus { get; set; }
        public DateTimeOffset? PackageScanResultDate { get; set; }
        public string PackageScanFlagResult { get; set; }
    }
}
