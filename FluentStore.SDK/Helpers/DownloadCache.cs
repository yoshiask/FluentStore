using Garfoot.Utilities.FluentUrn;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace FluentStore.SDK.Helpers
{
    public class DownloadCache
    {
        public DownloadCache(DirectoryInfo directory = null, bool createIfDoesNotExist = true)
        {
            directory ??= StorageHelper.GetTempDirectory();

            CacheDatabase = new(Path.Combine(directory.FullName, CACHE_FILENAME));
            if (!CacheDatabase.Exists && createIfDoesNotExist)
            {
                using BinaryWriter cache = new(CacheDatabase.Open(FileMode.CreateNew));
                cache.Write(FILE_MAGIC);
            }
        }

        private static readonly byte[] FILE_MAGIC = new byte[] { 0x46, 0x6C, 0x53, 0x74 };
        private const string CACHE_FILENAME = "cache.dat";
        private FileInfo CacheDatabase = null;

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

        private bool DoesCacheExist()
        {
            if (CacheDatabase == null) return false;
            CacheDatabase.Refresh();
            return CacheDatabase.Exists;
        }

        public void Clear()
        {
            if (!DoesCacheExist()) return;

            CacheDatabase.Directory.RecursiveDelete();
            CacheDatabase.Refresh();
        }

        public void Add(Urn urn, string version, FileSystemInfo downloadItem)
        {
            if (TryGet(urn, out var cachedEntry) && version != cachedEntry.Value.GetVersion())
            {
                Remove(urn);
            }

            CacheEntry entry = new()
            {
                urnBytes = Encoding.Default.GetBytes(urn.ToString()),
                versionBytes = Encoding.Default.GetBytes(version.ToString()),
                downloadItemBytes = Encoding.Default.GetBytes(downloadItem.FullName),
            };

            // Write lengths, then data
            using BinaryWriter file = new(CacheDatabase.Open(FileMode.Append));
            entry.WriteToStream(file);
        }

        public void Remove(Urn urn)
        {
            if (!DoesCacheExist()) return;

            using FileStream cache = CacheDatabase.Open(FileMode.Open);
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
                        downloadItem.RecursiveDelete();

                    // Zero out entry, acts like a NOP slide
                    writer.BaseStream.Seek(entryPos, SeekOrigin.Begin);
                    for (int i = 0; i < entry.Length; i++)
                        writer.Write(byte.MinValue);

                    break;
                }
            }
        }

        public CacheEntry? Get(Urn urn)
        {
            if (!DoesCacheExist()) return null;

            using FileStream cache = CacheDatabase.Open(FileMode.Open);
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

        public bool TryGet(Urn urn, [NotNullWhen(true)] out CacheEntry? entry)
        {
            entry = null;
            try
            {
                entry = Get(urn);
            }
            catch { }

            return entry != null;
        }
    }
}
