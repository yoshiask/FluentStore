using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentStore.SDK.Helpers
{
    public class DownloadCache
    {
        public DownloadCache(DirectoryInfo directory = null, bool createIfDoesNotExist = true)
        {
            directory ??= StorageHelper.GetTempDirectoryPath();

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
            public int offsetToNext = 0;

            public long Length => 7 * sizeof(int) + urnLength + downloadItemLength;

            public void WriteToStream(BinaryWriter writer)
            {
                writer.Write(urnLength);
                writer.Write(versionLength);
                writer.Write(downloadItemLength);

                writer.Write(urnBytes);
                writer.Write(versionBytes);
                writer.Write(downloadItemBytes);

                writer.Write(offsetToNext);  // Relative offset to next item, from end of this one
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

                entry.offsetToNext = reader.ReadInt32();

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

        public void Clear()
        {
            if (CacheDatabase == null || !CacheDatabase.Exists) return;

            CacheDatabase.Delete();
            foreach (FileSystemInfo info in CacheDatabase.Directory.GetFileSystemInfos())
            {
                if (info is DirectoryInfo subdir)
                    subdir.RecursiveDelete();
                else
                    info.Delete();
            }
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
            if (CacheDatabase == null || !CacheDatabase.Exists) return;

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
                CacheEntry entry = CacheEntry.ReadFromStream(reader);

                // Check for equality
                if (entry.urnBytes.SequenceEqual(targetUrnBytes))
                {
                    // Overwrite "offsetToNext" on preview entry
                    // to skip over this entry
                    writer.BaseStream.Seek(-entry.Length - sizeof(int), SeekOrigin.Current);
                    writer.Write(entry.Length);

                    // Zero out entry, helps if cache is compressed later
                    for (int i = 0; i < entry.Length; i++)
                        writer.Write(0);

                    break;
                }
            }
        }

        public CacheEntry? Get(Urn urn)
        {
            if (CacheDatabase == null || !CacheDatabase.Exists) return null;

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
