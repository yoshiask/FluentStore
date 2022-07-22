using CommunityToolkit.Diagnostics;
using Newtonsoft.Json;
using System;
using System.IO;

namespace FluentStore.SDK.Models
{
    /// <summary>
    /// Contains metadata for a plugin.
    /// </summary>
    [Serializable]
    public sealed class PluginMetadata
    {
        public const string MetadataFileName = "metadata.json";

        /// <summary>
        /// Creates a new instance of <see cref="PluginMetadata"/>.
        /// </summary>
        /// <remarks>
        /// To be used for de/serialization.
        /// </remarks>
        public PluginMetadata()
        {

        }

        /// <summary>
        /// A unique identifier for this registered plugin.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The user-friendly name for the plugin.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The version of this plugin.
        /// </summary>
        public Version PluginVersion { get; set; }

        /// <summary>
        /// The version of the Fluent Store SDK that this plugin was built against.
        /// </summary>
        public Version SdkVersion { get; set; }

        /// <summary>
        /// A path (relative to the plugin root) to the main plugin DLL.
        /// </summary>
        public string PluginAssembly { get; set; }

        /// <summary>
        /// Reads the specified file and deserializes the JSON
        /// to an instance of <see cref="PluginMetadata"/>.
        /// </summary>
        public static PluginMetadata DeserializeFromFile(string path)
        {
            return JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(path));
        }

        /// <summary>
        /// Reads the reads and deserializes the JSON in the stream
        /// to an instance of <see cref="PluginMetadata"/>.
        /// </summary>
        public static PluginMetadata DeserializeFromStream(Stream stream)
        {
            Guard.CanRead(stream);

            byte[] bytes;

            if (stream.CanSeek)
            {
                bytes = new byte[stream.Length];
                stream.Read(bytes);
            }
            else
            {
                using MemoryStream memStream = new();
                stream.CopyTo(memStream);
                bytes = memStream.ToArray();
            }

            string json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<PluginMetadata>(json);
        }
    }
}
