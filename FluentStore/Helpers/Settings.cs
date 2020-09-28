using Windows.Storage;

namespace FluentStore.Helpers
{
    public class Settings : ObservableSettings
    {
        private static Settings settings = new Settings();
        public static Settings Default
        {
            get { return settings; }
        }

        public Settings()
            : base(ApplicationData.Current.LocalSettings)
        {
        }

        [DefaultSettingValue(Value = false)]
        public bool UseAppInstaller
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }
    }
}
