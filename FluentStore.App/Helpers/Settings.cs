using FluentStore.SDK.Helpers;
using FluentStore.Services;
using System;
using System.IO;
using Windows.Storage;

namespace FluentStore.Helpers
{
    public class Settings : ObservableSettings, ISettingsService
    {
        private const string KEY_PackageHandlerEnabled = "PackageHandlerEnabled";

        private static readonly Settings settings = new();
        public static Settings Default => settings;

        public Settings() : base(ApplicationData.Current.LocalSettings,
            new()
            {
                { KEY_PackageHandlerEnabled, ApplicationData.Current.LocalSettings.CreateContainer(KEY_PackageHandlerEnabled, ApplicationDataCreateDisposition.Always) }
            })
        {
        }

        public string ExclusionFilter
        {
            get => Get<string>();
            set => Set(value);
        }

        public bool UseExclusionFilter
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string PluginDirectory
        {
            get
            {
                var val = Get<string>();
                if (val == null)
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    DirectoryInfo dir = new(Path.Combine(localAppData, "FluentStoreBeta", "Plugins"));
                    if (!dir.Exists)
                        dir.Create();
                    PluginDirectory = val = dir.FullName;
                }
                return val;
            }
            set => Set(value);
        }

        public Version LastLaunchedVersion
        {
            get => Get<Version>();
            set => Set(value);
        }

        public bool GetPackageHandlerEnabledState(string typeName)
        {
            return Get<bool>(KEY_PackageHandlerEnabled, typeName, true);
        }

        public void SetPackageHandlerEnabledState(string typeName, bool enabled)
        {
            Set(KEY_PackageHandlerEnabled, enabled, typeName);
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
    }
}
