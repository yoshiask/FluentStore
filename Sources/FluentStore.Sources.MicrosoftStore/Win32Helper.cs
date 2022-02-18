using FluentStore.SDK.Models;
using System.Linq;
using System.Management;

namespace FluentStore.Handlers.MicrosoftStore
{
    internal static class Win32Helper
    {
        private const string REG_NTCURRENTVERSION = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public static WindowsUpdateLib.CTAC GetSystemInfo()
        {
            ObjectQuery query = new("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new(query);
            var info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

            var sku = (WindowsUpdateLib.OSSkuId)(int)(uint)info["OperatingSystemSKU"];
            var osVersion = info["Version"].ToString();
            var arch = SDK.Helpers.Win32Helper.GetSystemArchitecture() switch
            {
                Architecture.x86 => WindowsUpdateLib.MachineType.x86,
                Architecture.x64 => WindowsUpdateLib.MachineType.amd64,
                Architecture.Arm32 => WindowsUpdateLib.MachineType.arm,
                Architecture.Arm64 => WindowsUpdateLib.MachineType.arm64,

                _ => WindowsUpdateLib.MachineType.unknown
            };

            var ntCurrentVersion = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REG_NTCURRENTVERSION);
            var flightRing = ntCurrentVersion.GetValue("DisplayVersion").ToString();
            var flightBranch = ntCurrentVersion.GetValue("BuildBranch").ToString();
            osVersion += "." + ntCurrentVersion.GetValue("UBR").ToString();

            return new(sku, osVersion, arch, flightRing, flightRing, "CB", flightBranch, "Production", false, true);
        }
    }
}
