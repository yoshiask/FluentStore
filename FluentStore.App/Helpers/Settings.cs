using FluentStore.Services;
using Windows.Storage;

namespace FluentStore.Helpers
{
    public class Settings : ObservableSettings, ISettingsService
    {
        private static Settings settings = new Settings();
        public static Settings Default => settings;

        public Settings()
            : base(ApplicationData.Current.LocalSettings)
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
    }
}
