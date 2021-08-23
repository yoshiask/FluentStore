using WinGetRun.Enums;

namespace WinGetRun.Models
{
    public class Installer
    {
        public InstallerArchitecture Arch { get; set; }
        public string Url { get; set; }
        public string Sha256 { get; set; }
        public string? SignatureSha256 { get; set; }
        public string? Language { get; set; }
        public InstallerType? InstallerType { get; set; }
        public string? Scope { get; set; }
        public string? SystemAppId { get; set; }
        public Switches? Switches { get; set; }
    }
}
