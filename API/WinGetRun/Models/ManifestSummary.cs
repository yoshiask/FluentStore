namespace WinGetRun.Models
{
    public class ManifestSummary
    {
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string[] Tags { get; set; }
        public string? Description { get; set; }
        public string? Homepage { get; set; }
        public string? License { get; set; }
        public string? LicenseUrl { get; set; }
    }
}
