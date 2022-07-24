using OwlCore.AbstractStorage;
using System;
using System.Threading.Tasks;

namespace FluentStore.Services
{
    /// <summary>
    /// Provides <see cref="IFolderData"/> instances to common folder paths.
    /// </summary>
    public interface ICommonPathManager
    {
        /// <summary>
        /// Gets the app data folder for the current package and user.
        /// </summary>
        public Task<IFolderData> GetAppDataDirectoryAsync();

        /// <summary>
        /// Gets the default plugin directory.
        /// </summary>
        public Task<IFolderData> GetDefaultPluginDirectoryAsync();

        /// <summary>
        /// Gets the default settings directory.
        /// </summary>
        public Task<IFolderData> GetDefaultSettingsDirectoryAsync();

        /// <summary>
        /// Gets the default logs directory.
        /// </summary>
        public Task<IFolderData> GetDefaultLogDirectoryAsync();

        /// <summary>
        /// Gets the temporary directory.
        /// </summary>
        public Task<IFolderData> GetTempDirectoryAsync();

        /// <summary>
        /// Creates a folder the temporary directory using the relative path.
        /// </summary>
        /// <param name="relativePath">
        /// The folder structure to create inside the temporary directory.
        /// </param>
        public Task<IFolderData> CreateDirectoryTempAsync(string relativePath);

        /// <summary>
        /// Creates a new log file for the current session.
        /// </summary>
        /// <param name="timestamp">
        /// The time the session was started. Defaults to <see cref="DateTimeOffset.UtcNow"/>.
        /// </param>
        public Task<IFileData> CreateLogFileAsync(DateTimeOffset? timestamp = null);
    }
}
