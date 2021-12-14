using FluentStore.SDK.Images;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Messages;
using Windows.System;

namespace FluentStore.SDK.Helpers
{
    public static class Win32Helper
    {
        private const string REG_NTCURRENTVERSION = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public static ProcessorArchitecture GetSystemArchitecture()
        {
            PInvoke.Kernel32.GetNativeSystemInfo(out var sysInfo);
            return sysInfo.wProcessorArchitecture switch
            {
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL => ProcessorArchitecture.X86,
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64 => ProcessorArchitecture.X64,
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM => ProcessorArchitecture.Arm,
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM64 => ProcessorArchitecture.Arm64,

                _ => ProcessorArchitecture.Unknown,
            };
        }

        public static WindowsUpdateLib.CTAC GetSystemInfo()
        {
            ObjectQuery query = new("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new(query);
            var info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

            var sku = (WindowsUpdateLib.OSSkuId)(int)(uint)info["OperatingSystemSKU"];
            var osVersion = info["Version"].ToString();
            var arch = GetSystemArchitecture() switch
            {
                ProcessorArchitecture.X86 => WindowsUpdateLib.MachineType.x86,
                ProcessorArchitecture.X64 => WindowsUpdateLib.MachineType.amd64,
                ProcessorArchitecture.Arm => WindowsUpdateLib.MachineType.arm,
                ProcessorArchitecture.Arm64 => WindowsUpdateLib.MachineType.arm64,

                _ => WindowsUpdateLib.MachineType.unknown
            };

            var ntCurrentVersion = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REG_NTCURRENTVERSION);
            var flightRing = ntCurrentVersion.GetValue("DisplayVersion").ToString();
            var flightBranch = ntCurrentVersion.GetValue("BuildBranch").ToString();
            osVersion += "." + ntCurrentVersion.GetValue("UBR").ToString();

            return new(sku, osVersion, arch, flightRing, flightRing, "CB", flightBranch, "Production", false, true);
        }

        /// <inheritdoc cref="PackageBase.InstallAsync"/>
        public static async Task<bool> Install(PackageBase package, string args = null)
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(package));

                // Create and run new process for installer
                ProcessStartInfo startInfo = new()
                {
                    FileName = package.DownloadItem.FullName,
                    Arguments = args,
                    UseShellExecute = true,
                };
                var installProc = Process.Start(startInfo);
                await installProc.WaitForExitAsync();

                bool success = installProc.ExitCode == 0;
                if (success)
                    WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(package));
                else
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(
                        new Exception(installProc.ExitCode.ToString()), package, ErrorType.PackageInstallFailed));
                return success;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, package, ErrorType.PackageInstallFailed));
                var logger = Ioc.Default.GetRequiredService<Services.LoggerService>();
                logger.UnhandledException(ex, "Exception from Win32 component");
                return false;
            }
        }

        /// <inheritdoc cref="PackageBase.CacheAppIcon"/>
        public static async Task<ImageBase> GetAppIcon(FileInfo file)
        {
            // Open package archive for reading
            using FileStream stream = file.OpenRead();
            using ZipArchive archive = new(stream);

            // Get the app icon
            ZipArchiveEntry iconEntry = archive.Entries.FirstOrDefault(e => e.FullName.StartsWith(".rsrc/ICON/1"));
            if (iconEntry != null)
            {
                return new StreamImage
                {
                    ImageType = ImageType.Logo,
                    BackgroundColor = "Transparent",
                    Stream = iconEntry.Open()
                };
            }
            else
            {
                return null;
            }
        }
    }
}