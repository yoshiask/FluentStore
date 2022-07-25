using CommunityToolkit.Diagnostics;
using Garfoot.Utilities.FluentUrn;
using OwlCore.AbstractStorage;
using OwlCore.Provisos;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.SDK.Helpers
{
    public class DownloadCache : IAsyncInit
    {
        public DownloadCache(IFolderData directory, bool createIfDoesNotExist = true)
        {
            Guard.IsNotNull(directory, nameof(directory));

            CacheDirectory = directory;
            CreateIfDoesNotExist = createIfDoesNotExist;
        }

        private static readonly byte[] FILE_MAGIC = new byte[] { 0x46, 0x6C, 0x53, 0x74 };
        private const string CACHE_FILENAME = "cache.dat";
        private IFileData CacheDatabase = null;

        private readonly IFolderData CacheDirectory = null;
        private readonly bool CreateIfDoesNotExist;

        public bool IsInitialized { get; private set; }

        public struct CacheEntry
        {
            public int urnLength => urnBytes.Length;
            public int versionLength => versionBytes.Length;
            public int downloadItemLength => downloadItemBytes.Length;
            public byte[] urnBytes;
            public byte[] versionBytes;
            public byte[] downloadItemBytes;

            public long Length => 4 * sizeof(int) + urnLength + versionLength + downloadItemLength;

            public void WriteToStream(BinaryWriter writer)
            {
                writer.Write(byte.MaxValue);
                writer.Write(urnLength);
                writer.Write(versionLength);
                writer.Write(downloadItemLength);

                writer.Write(urnBytes);
                writer.Write(versionBytes);
                writer.Write(downloadItemBytes);
            }

            public static CacheEntry ReadFromStream(BinaryReader reader)
            {
                CacheEntry entry = new();

                int urnLength = reader.ReadInt32();
                int versionLength = reader.ReadInt32();
                int downloadItemLength = reader.ReadInt32();

                entry.urnBytes = reader.ReadBytes(urnLength);
                entry.versionBytes = reader.ReadBytes(versionLength);
                entry.downloadItemBytes = reader.ReadBytes(downloadItemLength);

                return entry;
            }

            public Urn GetUrn()
            {
                return Urn.Parse(Encoding.Default.GetString(urnBytes));
            }

            public string GetVersion()
            {
                return Encoding.Default.GetString(versionBytes);
            }

            public FileSystemInfo GetDownloadItem()
            {
                string path = Encoding.Default.GetString(downloadItemBytes);
                if (File.Exists(path))
                    return new FileInfo(path);
                else
                    return new DirectoryInfo(path);
            }
        }

        private bool DoesCacheExist() => CacheDatabase != null;

        public Task Clear()
        {
            if (!DoesCacheExist())
                return Task.CompletedTask;

            return CacheDirectory.RecursiveDelete();
        }

        public async Task Add(Urn urn, string version, IFileData downloadItem)
        {
            (bool success, var cachedEntry) = await TryGet(urn);
            if (success && version != cachedEntry.Value.GetVersion())
            {
                await Remove(urn);
            }

            CacheEntry entry = new()
            {
                urnBytes = Encoding.Default.GetBytes(urn.ToString()),
                versionBytes = Encoding.Default.GetBytes(version.ToString()),
                downloadItemBytes = Encoding.Default.GetBytes(downloadItem.Path),
            };

            // Write lengths, then data
            using Stream stream = await CacheDatabase.GetStreamAsync(FileAccessMode.ReadWrite);
            using BinaryWriter file = new(stream);
            entry.WriteToStream(file);
        }

        public async Task Remove(Urn urn)
        {
            if (!DoesCacheExist()) return;

            using Stream cache = await CacheDatabase.GetStreamAsync(FileAccessMode.ReadWrite);
            using BinaryReader reader = new(cache);
            using BinaryWriter writer = new(cache);
            // Skip file header
            reader.BaseStream.Seek(FILE_MAGIC.Length, SeekOrigin.Begin);

            // Loop through items in cache until target URN is found
            byte[] targetUrnBytes = Encoding.Default.GetBytes(urn.ToString());
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Read entry
                long entryPos = writer.BaseStream.Position;
                if (reader.ReadByte() != byte.MaxValue) continue;

                CacheEntry entry = CacheEntry.ReadFromStream(reader);

                // Check for equality
                if (entry.urnBytes.SequenceEqual(targetUrnBytes))
                {
                    // Delete cached file, if it exists
                    var downloadItem = entry.GetDownloadItem();
                    if (downloadItem.Exists)
                    {
                        if (downloadItem is DirectoryInfo dir)
                            dir.Delete(true);
                        else
                            downloadItem.Delete();
                    }

                    // Zero out entry, acts like a NOP slide
                    writer.BaseStream.Seek(entryPos, SeekOrigin.Begin);
                    for (int i = 0; i < entry.Length; i++)
                        writer.Write(byte.MinValue);

                    break;
                }
            }
        }

        public async Task<CacheEntry?> Get(Urn urn)
        {
            if (!DoesCacheExist()) return null;

            using Stream cache = await CacheDatabase.GetStreamAsync(FileAccessMode.ReadWrite);
            using BinaryReader reader = new(cache);
            using BinaryWriter writer = new(cache);
            // Skip file header
            reader.BaseStream.Seek(FILE_MAGIC.Length, SeekOrigin.Begin);

            // Loop through items in cache until target URN is found
            byte[] targetUrnBytes = Encoding.Default.GetBytes(urn.ToString());
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Read entry
                if (reader.ReadByte() != byte.MaxValue) continue;
                CacheEntry entry = CacheEntry.ReadFromStream(reader);

                // Check for equality
                if (entry.urnBytes.SequenceEqual(targetUrnBytes))
                {
                    return entry;
                }
            }

            return null;
        }

        public async Task<(bool success, CacheEntry? entry)> TryGet(Urn urn)
        {
            CacheEntry? entry = null;
            try
            {
                entry = await Get(urn);
            }
            catch { }

            return (entry != null, entry);
        }

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            CacheDatabase = await CacheDirectory.GetFileAsync(CACHE_FILENAME);

            if (CacheDatabase == null && CreateIfDoesNotExist)
            {
                CacheDatabase = await CacheDirectory.CreateFileAsync(CACHE_FILENAME);

                using Stream stream = await CacheDatabase.GetStreamAsync(FileAccessMode.ReadWrite);
                using BinaryWriter cache = new(stream);
                cache.Write(FILE_MAGIC);
            }
        }
    }
}
