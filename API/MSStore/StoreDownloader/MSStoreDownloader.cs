/// Courtesy of Gustave Monce-- https://github.com/gus33000

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads;

namespace StoreDownloader
{
    public static class MSStoreDownloader
    {
        public static async Task<string[]> DownloadPackageAsync(string[] categoryIds, CTAC ctac,
            DirectoryInfo downloadDirectory, DownloadProgress progress, string msaToken = "")
        {
            var updates = await FE3Handler.GetUpdates(categoryIds, ctac, msaToken, FileExchangeV3UpdateFilter.Application);
            return await DownloadPackageAsync(updates, downloadDirectory, progress, msaToken);
        }

        public static async Task<string[]> DownloadPackageAsync(IEnumerable<UpdateData> updates,
            DirectoryInfo downloadDirectory, DownloadProgress progress, string msaToken = "", bool downloadAll = false)
        {
            List<ApplicationFile> appfiles = new();
            List<ApplicationFile> appdepfiles = new();

            foreach (var updateData in updates)
            {
                foreach (var file in updateData.Xml.Files.File)
                {
                    if (file.FileName.EndsWith(".cab", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var filename = updateData.AppxMetadata.ContentPackageId + "." +
                        file.FileName.Split(".")[file.FileName.Count(x => x == '.')];

                    if (string.IsNullOrEmpty(updateData.AppxMetadata.ContentPackageId))
                    {
                        filename = file.FileName;
                    }

                    var datetime = DateTime.Parse(file.Modified);

                    if (!appfiles.Any(x => x.Filename == filename))
                    {
                        ApplicationFile appfile = new()
                        {
                            UpdateID = updateData.Xml.UpdateIdentity.UpdateID,
                            RevisionNumber = updateData.Xml.UpdateIdentity.RevisionNumber,
                            Token = msaToken,
                            Digest = file.Digest,
                            Filename = filename,
                            UpdateData = updateData,
                            Modified = datetime,
                            CTAC = updateData.CTAC,
                            Size = file.Size
                        };

                        List<string> descs = new();

                        if (updateData != null && updateData.AppxMetadata != null && updateData.AppxMetadata.ContentTargetPlatforms != null)
                        {
                            foreach (var plat in updateData.AppxMetadata.ContentTargetPlatforms)
                            {
                                var max = GetFriendlyVersionFromULong((ulong)plat.PlatformMaxVersionTested);
                                var min = GetFriendlyVersionFromULong((ulong)plat.PlatformMinVersion);
                                var target = GetFriendlyPlatformNameFromLong(plat.PlatformTarget);

                                var desc = $"{target} - Minimum: {min} Maximum: {max}";
                                descs.Add(desc);
                            }
                        }

                        appfile.Targets = descs.ToArray();

                        if (updateData.Xml.ExtendedProperties.IsAppxFramework == "true")
                        {
                            appdepfiles.Add(appfile);
                        }
                        else
                        {
                            appfiles.Add(appfile);
                        }
                    }
                }
            }

            appfiles.Sort((x, y) => x.Modified.CompareTo(y.Modified));

            List<FileExchangeV3FileDownloadInformation> files = new();

            foreach (var update in updates)
            {
                files.AddRange(await FE3Handler.GetFileUrls(update, msaToken));
            }

            IEnumerable<ApplicationFile> filteredAppFiles;
            if (downloadAll)
            {
                filteredAppFiles = appfiles.Concat(appdepfiles);
            }
            else
            {
                filteredAppFiles = appfiles.Where(app => !app.Targets.Any(t => t.Contains("Xbox")));
            }

            List<UUPFile> fileList = filteredAppFiles.Select(boundApp =>
            {
                return new UUPFile(
                    files.First(x => x.Digest == boundApp.Digest),
                    boundApp.Filename,
                    long.Parse(boundApp.Size),
                    boundApp.Digest,
                    "sha1");
            }).Reverse().ToList();

            using var helperDl = new HttpDownloader(downloadDirectory.FullName);
            return await helperDl.DownloadAsync(fileList, progress).ConfigureAwait(false)
                ? fileList.Select(f => f.FileName).ToArray() : null;
        }

        public static string GetFriendlyVersionFromULong(ulong value)
        {
            ulong major = (value & 0xFFFF000000000000L) >> 48;
            ulong minor = (value & 0x0000FFFF00000000L) >> 32;
            ulong build = (value & 0x00000000FFFF0000L) >> 16;
            ulong revision = (value & 0x000000000000FFFFL);
            var osVersion = $"{major}.{minor}.{build}.{revision}";
            return osVersion;
        }

        public static string GetFriendlyPlatformNameFromLong(long value)
        {
            return value switch
            {
                0 => "Windows Universal",
                3 => "Windows Desktop",
                4 => "Windows Mobile",
                5 => "Windows Xbox",
                6 => "Windows Team",
                10 => "Windows Holographic",
                16 => "Windows Core",

                _ => $"Unknown ({value})",
            };
        }
    }
}
