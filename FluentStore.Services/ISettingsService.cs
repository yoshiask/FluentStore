using OwlCore.Diagnostics;
using System;
using System.Collections.Generic;

namespace FluentStore.Services
{
    public interface ISettingsService
    {
        public string ExclusionFilter { get; set; }

        public bool UseExclusionFilter { get; set; }

        public string PluginDirectory { get; set; }

        public Version LastLaunchedVersion { get; set; }

        public LogLevel LoggingLevel { get; set; }

        public Dictionary<string, bool> PackageHandlerEnabled { get; set; }

        /// <summary>
        /// Compares <see cref="LastLaunchedVersion"/> to the current app's version.
        /// </summary>
        public AppUpdateStatus GetAppUpdateStatus();
    }

    public class DefaultSettingValues
    {
        public DefaultSettingValues() { }

        public virtual string ExclusionFilter() => @"(?i)(guide|manual|tutorial)(?-i)";

        public virtual bool UseExclusionFilter() => true;

        public virtual string PluginDirectory()
        {
            System.IO.Directory.CreateDirectory(CommonPaths.DefaultPluginDirectoryName);
            return CommonPaths.DefaultPluginDirectoryName;
        }

        public virtual Version LastLaunchedVersion() => null;

        public virtual LogLevel LoggingLevel() =>
#if DEBUG
            LogLevel.Warning;
#else
            LogLevel.Critical;
#endif
    }

    public enum AppUpdateStatus : byte
    {
        /// <summary>
        /// The app was not updated, nothing changed.
        /// </summary>
        None,

        /// <summary>
        /// The app has not been launched since the last update.
        /// </summary>
        Updated,

        /// <summary>
        /// The app has been launched for the first time.
        /// </summary>
        NewlyInstalled,

        /// <summary>
        /// A previous version of the app has been installed.
        /// </summary>
        Downgraded,
    }

    public class PackageHandlerEnabledStateChangedEventArgs : EventArgs
    {
        public string TypeName { get; }
        public bool NewState { get; }

        public PackageHandlerEnabledStateChangedEventArgs(string typeName, bool newState)
        {
            TypeName = typeName;
            NewState = newState;
        }
    }
}
