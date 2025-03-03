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
using FluentStore.SDK.Models;
using System.Collections.Generic;

namespace FluentStore.SDK.Helpers
{
    public static class Win32Helper
    {
        public static readonly bool IsWindows19041OrGreater = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041);

        public static Architecture GetSystemArchitecture()
        {
            PInvoke.Kernel32.GetNativeSystemInfo(out var sysInfo);
            return sysInfo.wProcessorArchitecture switch
            {
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL => Architecture.x86,
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64 => Architecture.x64,
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM => Architecture.Arm32,
                PInvoke.Kernel32.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM64 => Architecture.Arm64,

                _ => Architecture.Unknown,
            };
        }

        /// <inheritdoc cref="PackageBase.InstallAsync"/>
        public static async Task<bool> Install(PackageBase package, string args = null,
            IEnumerable<long> successCodes = null, IEnumerable<long> errorCodes = null,
            Func<long, string> getErrorMessage = null)
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

                successCodes ??= new long[] { 0 };
                errorCodes ??= Array.Empty<long>();
                long exitCode = installProc.ExitCode;
                bool success = successCodes.Contains(exitCode) && !errorCodes.Contains(exitCode);

                if (success)
                {
                    WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(package));
                }
                else
                {
                    getErrorMessage ??= GetInstallErrorMessage;
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(
                        new Exception(getErrorMessage(exitCode)),
                        package, ErrorType.PackageInstallFailed));
                }
                
                return success;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, package, ErrorType.PackageInstallFailed));
                var logger = Ioc.Default.GetRequiredService<Services.LoggerService>();
                logger.UnhandledException(ex, Microsoft.Extensions.Logging.LogLevel.Error);
                return false;
            }
        }

        /// <inheritdoc cref="PackageBase.CacheAppIcon"/>
        public static ImageBase GetAppIcon(Stream stream)
        {
            ZipArchive archive = null;

            try
            {
                // Open package archive for reading
                archive = new(stream);

                // Get the app icon
                ZipArchiveEntry iconEntry = archive.Entries.FirstOrDefault(e => e.FullName.StartsWith(".rsrc/ICON/1"));
                if (iconEntry != null)
                {
                    // Copy the image to memory so the archive can be closed
                    using var iconStream = iconEntry.Open();
                    MemoryStream memIconStream = new();
                    iconStream.CopyTo(memIconStream);

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
            finally
            {
                archive?.Dispose();
            }
            
        }

        private static string GetInstallErrorMessage(long exitCode)
            => $"Installer exited with code {exitCode}.";
    }
}