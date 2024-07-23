using FluentStore.SDK.Helpers;
using FluentStore.Services;
using OwlCore.ComponentModel;
using OwlCore.Diagnostics;
using OwlCore.Services;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System;
using System.IO;
using Windows.ApplicationModel;

namespace FluentStore.Helpers
{
    public class Settings : SettingsBase, ISettingsService
    {
        private const string KEY_PackageHandlerEnabled = "PackageHandlerEnabled";
        private static Settings s_settings;
        private static readonly DefaultSettingValues s_defVals = new();
        private readonly ICommonPathManager m_pathManager;

        public static Settings Default => s_settings;

        public Settings(ICommonPathManager pathManager) : base(GetSettingsFolder(pathManager), new NewtonsoftStreamSerializer())
        {
            m_pathManager = pathManager;
            s_settings = this;
        }

        public string ExclusionFilter
        {
            get => GetSetting(s_defVals.ExclusionFilter);
            set => SetSetting(value);
        }

        public bool UseExclusionFilter
        {
            get => GetSetting(s_defVals.UseExclusionFilter);
            set => SetSetting(value);
        }

        public string PluginDirectory
        {
            get => GetSetting(GetPluginDirectory);
            set => SetSetting(value);
        }

        public Version LastLaunchedVersion
        {
            get => GetSetting(s_defVals.LastLaunchedVersion);
            set => SetSetting(value);
        }

        public LogLevel LoggingLevel
        {
            get => GetSetting(s_defVals.LoggingLevel);
            set => SetSetting(value);
        }

        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
				return false;
#endif
            }
        }

        public string VersionString
        {
            get
            {
                PackageVersion ver = Package.Current.Id.Version;
                return $"{ver.Major}.{ver.Minor}.{ver.Build}";
            }
        }

        public event EventHandler<PackageHandlerEnabledStateChangedEventArgs> PackageHandlerEnabledStateChanged;

        public bool GetPackageHandlerEnabledState(string typeName)
        {
            return GetSetting(() => true, GetPackageHandlerEnabledKey(typeName));
        }

        public void SetPackageHandlerEnabledState(string typeName, bool enabled)
        {
            SetSetting(enabled, GetPackageHandlerEnabledKey(typeName));

            PackageHandlerEnabledStateChanged?.Invoke(this, new(typeName, enabled));
        }

        public AppUpdateStatus GetAppUpdateStatus()
        {
            if (LastLaunchedVersion == null)
                return AppUpdateStatus.NewlyInstalled;

            Version cur = Package.Current.Id.Version.ToVersion();
            if (LastLaunchedVersion == cur)
                return AppUpdateStatus.None;
            else if (LastLaunchedVersion < cur)
                return AppUpdateStatus.Updated;
            else if (LastLaunchedVersion > cur)
                return AppUpdateStatus.Downgraded;

            return AppUpdateStatus.None;
        }

        private static string GetPackageHandlerEnabledKey(string typeName) => $"{KEY_PackageHandlerEnabled}_{typeName}";

        private static IModifiableFolder GetSettingsFolder(ICommonPathManager pathManager)
        {
            SystemFolder dir = new(pathManager.GetDefaultSettingsDirectory());
            Directory.CreateDirectory(dir.Path);
            return dir;
        }

        private string GetPluginDirectory() => m_pathManager.GetDefaultPluginDirectory().FullName;
    }
}
