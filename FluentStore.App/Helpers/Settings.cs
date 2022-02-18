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

        public bool GetPackageHandlerEnabledState(string typeName)
        {
            return Get<bool>(KEY_PackageHandlerEnabled, typeName, true);
        }

        public void SetPackageHandlerEnabledState(string typeName, bool enabled)
        {
            Set(KEY_PackageHandlerEnabled, enabled, typeName);
        }
    }
}
