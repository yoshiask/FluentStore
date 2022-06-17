using System;
using System.Threading.Tasks;

namespace FluentStore.Services
{
    public interface ISettingsService
    {
        public string ExclusionFilter { get; set; }

        public bool UseExclusionFilter { get; set; }

        public string PluginDirectory { get; set; }

        public Version LastLaunchedVersion { get; set; }

        public event EventHandler<PackageHandlerEnabledStateChangedEventArgs> PackageHandlerEnabledStateChanged;

        /// <summary>
        /// Gets the enabled state of the specified package handler. Defaults to <c>true</c>.
        /// </summary>
        public bool GetPackageHandlerEnabledState(string typeName);

        /// <summary>
        /// Sets the enabled state of the specified package handler.
        /// </summary>
        public void SetPackageHandlerEnabledState(string typeName, bool enabled);

        /// <summary>
        /// Compares <see cref="LastLaunchedVersion"/> to the current app's version.
        /// </summary>
        public AppUpdateStatus GetAppUpdateStatus();

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        /// <returns></returns>
        public Task ClearSettings();
    }

    public class DefaultSettingValues
    {
        public DefaultSettingValues() { }

        public virtual string ExclusionFilter() => @"(?i)(guide|manual|tutorial)(?-i)";

        public virtual bool UseExclusionFilter() => true;

        public virtual string PluginDirectory()
        {
            System.IO.Directory.CreateDirectory(CommonPaths.DefaultPluginDirectory);
            return CommonPaths.DefaultPluginDirectory;
        }

        public virtual Version LastLaunchedVersion() => null;
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
