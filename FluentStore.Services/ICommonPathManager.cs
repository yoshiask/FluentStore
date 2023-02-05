using System;
using System.IO;

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
        public DirectoryInfo GetAppDataDirectory();

        /// <summary>
        /// Gets the default plugin directory.
        /// </summary>
        public DirectoryInfo GetDefaultPluginDirectory();

        /// <summary>
        /// Gets the default settings directory.
        /// </summary>
        public DirectoryInfo GetDefaultSettingsDirectory();

        /// <summary>
        /// Gets the default logs directory.
        /// </summary>
        public DirectoryInfo GetDefaultLogDirectory();

        /// <summary>
        /// Gets the temporary directory.
        /// </summary>
        public DirectoryInfo GetTempDirectory();

        /// <summary>
        /// Creates a folder the temporary directory using the relative path.
        /// </summary>
        /// <param name="relativePath">
        /// The folder structure to create inside the temporary directory.
        /// </param>
        public DirectoryInfo CreateDirectoryTemp(string relativePath);

        /// <summary>
        /// Creates a new log file for the current session.
        /// </summary>
        /// <param name="timestamp">
        /// The time the session was started. Defaults to <see cref="DateTimeOffset.UtcNow"/>.
        /// </param>
        public FileInfo CreateLogFile(DateTimeOffset? timestamp = null);
    }
}
