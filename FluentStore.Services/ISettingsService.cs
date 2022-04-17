using System;

namespace FluentStore.Services
{
    public interface ISettingsService
    {
        [DefaultSettingValue(Value = @"(?i)(guide|manual|tutorial)(?-i)")]
        public string ExclusionFilter { get; set; }

        [DefaultSettingValue(Value = true)]
        public bool UseExclusionFilter { get; set; }

        [DefaultSettingValue(Value = null)]
        public string PluginDirectory { get; set; }

        [DefaultSettingValue(Value = null)]
        public Version LastLaunchedVersion { get; set; }

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
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DefaultSettingValueAttribute : Attribute
    {
        public DefaultSettingValueAttribute()
        {
        }

        public DefaultSettingValueAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
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
}
