using FluentStore.SDK.Images;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Messages;

namespace FluentStore.SDK.Helpers
{
    public static class Win32Helper
    {
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
                    WeakReferenceMessenger.Default.Send(new PackageInstallCompletedMessage(package));
                else
                    WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(package, new Exception(installProc.ExitCode.ToString())));
                return success;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new PackageInstallFailedMessage(package, ex));
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