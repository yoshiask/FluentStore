using FluentStore.SDK.Images;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using OwlCore.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace FluentStore.SDK.Helpers
{
    public static class Win32Helper
    {
        public static async Task InvokeWin32ComponentAsync(string applicationPath, string arguments = null, bool runAsAdmin = false, string workingDirectory = null)
        {
            await InvokeWin32ComponentsAsync(applicationPath.IntoList(), arguments, runAsAdmin, workingDirectory);
        }

        public static async Task InvokeWin32ComponentsAsync(IEnumerable<string> applicationPaths, string arguments = null, bool runAsAdmin = false, string workingDirectory = null)
        {
            throw new NotImplementedException();
            Debug.WriteLine("Launching EXE in FullTrustProcess");

            object connection = null;
            if (connection != null)
            {
                var value = new ValueSet()
                {
                    { "Arguments", "LaunchApp" },
                    { "WorkingDirectory", workingDirectory },
                    { "Application", applicationPaths.FirstOrDefault() },
                    { "ApplicationList", JsonConvert.SerializeObject(applicationPaths) },
                };

                if (runAsAdmin)
                {
                    value.Add("Parameters", "runas");
                }
                else
                {
                    value.Add("Parameters", arguments);
                }

                //await connection.SendMessageAsync(value);
            }
        }

        /// <inheritdoc cref="PackageBase.InstallAsync"/>
        public static async Task<bool> Install(PackageBase package, string args = null)
        {
            try
            {
                await InvokeWin32ComponentAsync(package.DownloadItem.Path, args, true);
                return true;
            }
            catch (Exception ex)
            {
                var logger = Ioc.Default.GetRequiredService<Services.LoggerService>();
                logger.UnhandledException(ex, "Exception from Win32 component");
                return false;
            }
        }

        /// <inheritdoc cref="PackageBase.GetAppIcon"/>
        public static async Task<ImageBase> GetAppIcon(StorageFile file)
        {
            // Open package archive for reading
            using var stream = await file.OpenReadAsync();
            using var archive = new ZipArchive(stream.AsStream());

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