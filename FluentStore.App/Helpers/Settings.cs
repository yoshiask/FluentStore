using FluentStore.SDK.Helpers;
using FluentStore.Services;
using OwlCore.AbstractStorage;
using OwlCore.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace FluentStore.Helpers
{
    public class Settings : SettingsBase, ISettingsService
    {
        private const string KEY_PackageHandlerEnabled = "PackageHandlerEnabled";
        private static readonly Settings settings = new();
        private static readonly DefaultSettingValues defVals = new();

        public static Settings Default => settings;

        public Settings() : base(GetSettingsFolder(), new NewtonsoftStreamSerializer())
        {
        }

        public string ExclusionFilter
        {
            get => GetSetting(defVals.ExclusionFilter);
            set => SetSetting(value);
        }

        public bool UseExclusionFilter
        {
            get => GetSetting(defVals.UseExclusionFilter);
            set => SetSetting(value);
        }

        public string PluginDirectory
        {
            get => GetSetting(defVals.PluginDirectory);
            set => SetSetting(value);
        }

        public Version LastLaunchedVersion
        {
            get => GetSetting(defVals.LastLaunchedVersion);
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

        public bool GetPackageHandlerEnabledState(string typeName)
        {
            return GetSetting(() => true, GetPackageHandlerEnabledKey(typeName));
        }

        public void SetPackageHandlerEnabledState(string typeName, bool enabled)
        {
            SetSetting(enabled, GetPackageHandlerEnabledKey(typeName));
        }

        public AppUpdateStatus GetAppUpdateStatus()
        {
            if (LastLaunchedVersion == null)
                return AppUpdateStatus.NewlyInstalled;

            Version cur = Windows.ApplicationModel.Package.Current.Id.Version.ToVersion();
            if (LastLaunchedVersion == cur)
                return AppUpdateStatus.None;
            else if (LastLaunchedVersion < cur)
                return AppUpdateStatus.Updated;
            else if (LastLaunchedVersion > cur)
                return AppUpdateStatus.Downgraded;

            return AppUpdateStatus.None;
        }

        public async Task ClearSettings()
        {
            DirectoryInfo dir;
            if (Folder is SystemIOFolderData folder)
                dir = folder.Directory;
            else
                dir = new(Folder.Path);

            await Task.Run(dir.RecursiveDelete);
        }

        private static string GetPackageHandlerEnabledKey(string typeName) => $"{KEY_PackageHandlerEnabled}_{typeName}";

        private static SystemIOFolderData GetSettingsFolder()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            SystemIOFolderData dir = new(Path.Combine(localAppData, "FluentStoreBeta", "Settings"));

            dir.EnsureExists();
            return dir;
        }
    }
}
