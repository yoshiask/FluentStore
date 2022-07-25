using FluentStore.Services;
using OwlCore.AbstractStorage;
using System;
using System.Threading.Tasks;

namespace FluentStore.Helpers
{
    /// <summary>
    /// An implementation of <see cref="ICommonPathManager"/> using
    /// <see cref="Windows.Storage.StorageFile"/> APIs for packaged Windows apps.
    /// </summary>
    public sealed class PackagedPathManager : ICommonPathManager
    {
        public async Task<IFolderData> CreateDirectoryTempAsync(string relativePath)
        {
            var tempDir = await GetTempDirectoryAsync();

            foreach (string folder in relativePath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                tempDir = await tempDir.CreateFolderAsync(folder);

            return tempDir;
        }

        public async Task<IFileData> CreateLogFileAsync(DateTimeOffset? timestamp = null)
        {
            timestamp ??= DateTimeOffset.UtcNow;

            string logFileName = $"Log_{timestamp:yyyy-MM-dd_HH-mm-ss-fff}.log";

            var logDir = await GetDefaultLogDirectoryAsync();
            return await logDir.CreateFileAsync(logFileName, CreationCollisionOption.GenerateUniqueName);
        }

        public Task<IFolderData> GetAppDataDirectoryAsync()
        {
            StorageFolderData appDataDir = new(Windows.Storage.ApplicationData.Current.LocalFolder);

            // Ensure the directory exists.
            System.IO.Directory.CreateDirectory(appDataDir.Path);

            return Task.FromResult<IFolderData>(appDataDir);
        }

        public async Task<IFolderData> GetDefaultLogDirectoryAsync()
        {
            var appDataDir = await GetAppDataDirectoryAsync();
            return await appDataDir.CreateFolderAsync(CommonPaths.DefaultLogsDirectoryName, CreationCollisionOption.OpenIfExists);
        }

        public async Task<IFolderData> GetDefaultPluginDirectoryAsync()
        {
            var appDataDir = await GetAppDataDirectoryAsync();
            return await appDataDir.CreateFolderAsync(CommonPaths.DefaultPluginDirectoryName, CreationCollisionOption.OpenIfExists);
        }

        public async Task<IFolderData> GetDefaultSettingsDirectoryAsync()
        {
            var appDataDir = await GetAppDataDirectoryAsync();
            return await appDataDir.CreateFolderAsync(CommonPaths.DefaultSettingsDirectoryName, CreationCollisionOption.OpenIfExists);
        }

        public Task<IFolderData> GetTempDirectoryAsync()
        {
            StorageFolderData cacheDir = new(Windows.Storage.ApplicationData.Current.LocalCacheFolder);

            // Ensure directory exists.
            System.IO.Directory.CreateDirectory(cacheDir.Path);

            return Task.FromResult<IFolderData>(cacheDir);
        }
    }
}
