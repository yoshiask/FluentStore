namespace WinGetRun.Models
{
    public class Installer
    {
        public string Arch { get; set; }
        public string Url { get; set; }
        public string Sha256 { get; set; }
        public string? SignatureSha256 { get; set; }
        public string? Language { get; set; }
        public string? InstallerType { get; set; }
        public string? Scope { get; set; }
        public string? SystemAppId { get; set; }
        public Switches? Switches { get; set; }
    }
}
