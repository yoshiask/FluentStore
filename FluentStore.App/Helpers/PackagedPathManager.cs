using FluentStore.Services;
using System;
using System.IO;

namespace FluentStore.Helpers
{
    /// <summary>
    /// An implementation of <see cref="ICommonPathManager"/> using
    /// <see cref="Windows.Storage.StorageFile"/> APIs for packaged Windows apps.
    /// </summary>
    public sealed class PackagedPathManager : ICommonPathManager
    {
        public DirectoryInfo CreateDirectoryTemp(string relativePath)
        {
            var tempDir = GetTempDirectory();

            foreach (string folder in relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                tempDir = tempDir.CreateSubdirectory(folder);

            return tempDir;
        }

        public FileInfo CreateLogFile(DateTimeOffset? timestamp = null)
        {
            timestamp ??= DateTimeOffset.UtcNow;

            string logFileName = $"Log_{timestamp:yyyy-MM-dd_HH-mm-ss-fff}.log";

            var logDir = GetDefaultLogDirectory();
            return new(Path.Combine(logDir.FullName, logFileName));
        }

        public DirectoryInfo GetAppDataDirectory()
        {
            DirectoryInfo appDataDir = new(Windows.Storage.ApplicationData.Current.LocalFolder.Path);

            // Ensure the directory exists.
            Directory.CreateDirectory(appDataDir.FullName);

            return appDataDir;
        }

        public DirectoryInfo GetDefaultLogDirectory()
        {
            var appDataDir = GetAppDataDirectory();
            return appDataDir.CreateSubdirectory(CommonPaths.DefaultLogsDirectoryName);
        }

        public DirectoryInfo GetDefaultPluginDirectory()
        {
            var appDataDir = GetAppDataDirectory();
            return appDataDir.CreateSubdirectory(CommonPaths.DefaultPluginDirectoryName);
        }

        public DirectoryInfo GetDefaultSettingsDirectory()
        {
            var appDataDir = GetAppDataDirectory();
            return appDataDir.CreateSubdirectory(CommonPaths.DefaultSettingsDirectoryName);
        }

        public DirectoryInfo GetTempDirectory()
        {
            DirectoryInfo cacheDir = new(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path);

            // Ensure directory exists.
            Directory.CreateDirectory(cacheDir.FullName);

            return cacheDir;
        }
    }
}
