using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwlCore.AbstractStorage
{
    /// <summary>
    /// Represents a folder backed by <c>System.IO</c>.
    /// </summary>
    public class SystemIOFolderData : IFolderData
    {
        /// <summary>
        /// The underlying <see cref="DirectoryInfo"/> instance in use.
        /// </summary>
        public DirectoryInfo Directory { get; private set; }

        /// <summary>
        /// Constructs a new instance of <see cref="IFolderData"/>.
        /// </summary>
        public SystemIOFolderData(DirectoryInfo folder)
        {
            Directory = folder;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="IFolderData"/>.
        /// </summary>
        public SystemIOFolderData(string folderPath) : this(new DirectoryInfo(folderPath))
        {

        }

        /// <inheritdoc/>
        public string Name => Directory.Name;

        /// <inheritdoc/>
        public string Path => Directory.FullName;

        /// <inheritdoc/>
        public string Id => FluentStore.SDK.Helpers.StorageHelper.GetFileId(Directory);

        /// <inheritdoc/>
        public Task<IEnumerable<IFileData>> GetFilesAsync()
        {
            return Task.Run<IEnumerable<IFileData>>(() => Directory.GetFiles().Select(x => new SystemIOFileData(x)));
        }

        /// <inheritdoc />
        public Task DeleteAsync() => Task.Run(Directory.Delete);

        /// <inheritdoc/>
        public Task<IFolderData> GetParentAsync()
        {
            return Task.FromResult<IFolderData>(new SystemIOFolderData(Directory.Parent));
        }

        /// <inheritdoc/>
        public Task<IFolderData> CreateFolderAsync(string desiredName)
        {
            return Task.Run<IFolderData>(() => new SystemIOFolderData(Directory.CreateSubdirectory(desiredName)));
        }

        /// <inheritdoc/>
        public Task<IFolderData> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            if (options != CreationCollisionOption.FailIfExists)
                throw new NotImplementedException();
            return CreateFolderAsync(desiredName);
        }

        /// <inheritdoc/>
        public Task<IFileData> CreateFileAsync(string desiredName)
        {
            return Task.Run<IFileData>(delegate
            {
                SystemIOFileData file = new(System.IO.Path.Combine(Path, desiredName));
                file.File.Create().Dispose();

                return file;
            });
        }

        /// <inheritdoc/>
        public Task<IFileData> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            return Task.Run<IFileData>(delegate
            {
                string name = desiredName;
                FileMode mode = FileMode.CreateNew;

                if (options == CreationCollisionOption.OpenIfExists)
                {
                    mode = FileMode.OpenOrCreate;
                }
                else if (options == CreationCollisionOption.GenerateUniqueName)
                {
                    string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(name);
                    string ext = System.IO.Path.GetExtension(name);
                    int i = 0;
                    while (File.Exists(System.IO.Path.Combine(Path, name)))
                    {
                        name = $"{nameNoExt} ({++i}){ext}";
                    }
                }
                else if (options == CreationCollisionOption.ReplaceExisting)
                {
                    mode = FileMode.Create;
                }

                SystemIOFileData file = new(System.IO.Path.Combine(Path, name));
                file.File.Open(mode).Dispose();

                return file;
            });
        }

        /// <inheritdoc/>
        public Task<IFolderData?> GetFolderAsync(string name)
        {
            return Task.Run<IFolderData?>(delegate
            {
                SystemIOFolderData folder = new(System.IO.Path.Combine(Path, name));
                return folder.Directory.Exists ? folder : null;
            });
        }

        /// <inheritdoc/>
        public Task<IFileData?> GetFileAsync(string name)
        {
            return Task.Run<IFileData?>(delegate
            {
                SystemIOFileData file = new(System.IO.Path.Combine(Path, name));
                return file.File.Exists ? file : null;
            });
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IFolderData>> GetFoldersAsync()
        {
            return Task.Run<IEnumerable<IFolderData>>(() => Directory.GetDirectories().Select(x => new SystemIOFolderData(x)));
        }

        /// <inheritdoc />
        public Task EnsureExists()
        {
            return Task.Run(delegate
            {
                if (Directory.Exists) return;
                Directory.Create();
            });
        }
    }
}
