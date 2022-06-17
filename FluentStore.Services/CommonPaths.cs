using System;
using System.IO;

namespace FluentStore.Services
{
    public static class CommonPaths
    {
        public static readonly string LocalAppData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FluentStoreBeta");

        public static readonly string DefaultPluginDirectory = Path.Combine(LocalAppData, "Plugins");

        public static readonly string DefaultSettingsDirectory = Path.Combine(LocalAppData, "Settings");

        public static readonly string DefaultLogsDirectory = Path.Combine(LocalAppData, "Logs");

        public static string GenerateLogFilePath()
        {
            return Path.Combine(DefaultLogsDirectory, $"Log_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm-ss-fff}.log");
        }
    }
}
