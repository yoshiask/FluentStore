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
            var fileList = await FetchFilesAsync(updates, msaToken);
            return await DownloadFilesAsync(fileList, downloadDirectory, progress);
        }

        public static async Task<List<UUPFile>> FetchFilesAsync(IEnumerable<UpdateData> updates, string msaToken = "", bool includeDeps = false, bool includeXbox = false)
        {
            List<ApplicationFile> appFiles = new();
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

                    if (!appFiles.Any(x => x.Filename == filename))
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
                            appFiles.Add(appfile);
                        }
                    }
                }
            }


            List<FileExchangeV3FileDownloadInformation> files = new();

            foreach (var update in updates)
            {
                files.AddRange(await FE3Handler.GetFileUrls(update, msaToken));
            }

            appFiles.Sort((x, y) => x.Modified.CompareTo(y.Modified));
            IEnumerable<ApplicationFile> filteredAppFiles = appFiles;

            if (includeDeps)
            {
                filteredAppFiles = filteredAppFiles.Concat(appdepfiles);
            }

            if (!includeXbox)
            {
                filteredAppFiles = filteredAppFiles
                    .Where(app => !app.Targets.All(t => t.Contains("Xbox")));
            }

            return filteredAppFiles.Select(boundApp =>
            {
                return new UUPFile(
                    files.First(x => x.Digest == boundApp.Digest),
                    boundApp.Filename,
                    long.Parse(boundApp.Size),
                    boundApp.Digest,
                    "sha1");
            }).ToList();
        }

        public static async Task<string[]> DownloadFilesAsync(List<UUPFile> fileList, DirectoryInfo downloadDirectory, IProgress<GeneralDownloadProgress> progress)
        {
            using var helperDl = new HttpDownloader(downloadDirectory.FullName);
            return await helperDl.DownloadAsync(fileList, progress).ConfigureAwait(false)
                ? fileList.Select(f => f.FileName).ToArray() : null;
        }

        public static Version GetVersionFromULong(ulong value)
        {
            var major = (int)(value >> 48) & 0xFFFF;
            var minor = (int)(value >> 32) & 0xFFFF;
            var build = (int)(value >> 16) & 0xFFFF;
            var revision = (int)value & 0xFFFF;
            return new(major, minor, build, revision);
        }

        public static string GetFriendlyVersionFromULong(ulong value) => GetVersionFromULong(value).ToString();

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
