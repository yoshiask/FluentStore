using FluentStore.SDK.Models;
using Microsoft.Win32;
using System.Linq;
using System.Management;
using UnifiedUpdatePlatform.Services.WindowsUpdate;

namespace FluentStore.Sources.Microsoft.Store
{
    internal static class SystemHelper
    {
        private const string REG_NTCURRENTVERSION = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public static CTAC GetSystemInfo()
        {
            ObjectQuery query = new("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new(query);
            var info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

            var sku = (OSSkuId)(int)(uint)info["OperatingSystemSKU"];
            var osVersion = info["Version"].ToString();
            var arch = SDK.Helpers.Win32Helper.GetSystemArchitecture() switch
            {
                Architecture.x86 => MachineType.x86,
                Architecture.x64 => MachineType.amd64,
                Architecture.Arm32 => MachineType.arm,
                Architecture.Arm64 => MachineType.arm64,

                _ => MachineType.unknown
            };

            var ntCurrentVersion = Registry.LocalMachine.OpenSubKey(REG_NTCURRENTVERSION);
            var flightRing = ntCurrentVersion.GetValue("DisplayVersion").ToString();
            var flightBranch = ntCurrentVersion.GetValue("BuildBranch").ToString();
            osVersion += "." + ntCurrentVersion.GetValue("UBR").ToString();

            return new(sku, osVersion, arch, flightRing, flightRing, "CB", flightBranch, "Production", false, true);
        }
    }
}
