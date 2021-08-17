using System.Collections.Generic;

namespace WinGetRun.Models
{
    public class Manifest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? AppMoniker { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string? Channel { get; set; }
        public string? Author { get; set; }
        public string? License { get; set; }
        public string? LicenseUrl { get; set; }
        public string? MinOSVersion { get; set; }
        public string? Description { get; set; }
        public string? Homepage { get; set; }
        public string? Tags { get; set; }
        public string? FileExtensions { get; set; }
        public string? Protocols { get; set; }
        public string? Commands { get; set; }
        public string? InstallerType { get; set; }
        public Switches? Switches { get; set; }
        public string? Log { get; set; }
        public string? InstallLocation { get; set; }
        public List<Installer> Installers { get; set; }
        public List<Localization>? Localization { get; set; }
    }
}
