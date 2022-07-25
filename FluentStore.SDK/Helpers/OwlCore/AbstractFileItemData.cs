using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OwlCore.AbstractStorage
{
    /// <summary>
    /// Wraps an AbstractStorage file or folder. Used as a workaround
    /// for AbstractStorage's lack of a base <see cref="FileSystemInfo"/>.
    /// </summary>
    public class AbstractFileItemData : IMutableFileData, IFolderData
    {
        private object _abstractData;

        /// <summary>
        /// Creates a new instance of <see cref="AbstractFileItemData"/> around the
        /// supplied AbstractStorage interface.
        /// </summary>
        /// <param name="data">
        /// The instance to wrap. Must be of type <see cref="IFileData"/> or <see cref="IFolderData"/>.
        /// </param>
        public AbstractFileItemData(object data)
        {
            var type = data.GetType();
            Guard.IsTrue(type.IsAssignableTo(typeof(IFileData)) || type.IsAssignableTo(typeof(IFolderData)));

            _abstractData = data;
        }

        public string Id => _abstractData switch
        {
            IFileData file => file.Id,
            IFolderData folder => folder.Id,

            _ => throw new NotImplementedException(),
        };

        public string Name => _abstractData switch
        {
            IFileData file => file.Name,
            IFolderData folder => folder.Name,

            _ => throw new NotImplementedException(),
        };

        public string Path => _abstractData switch
        {
            IFileData file => file.Path,
            IFolderData folder => folder.Path,

            _ => throw new NotImplementedException(),
        };

        public string DisplayName => _abstractData is IFileData file
            ? file.DisplayName : null;

        public string FileExtension => _abstractData is IFileData file
            ? file.FileExtension : null;

        public IFileDataProperties Properties
        {
            get => _abstractData is IFileData file
                ? file.Properties : null;
            set
            {
                if (_abstractData is IFileData file)
                    file.Properties = value;
                else
                    throw new NotSupportedException();
            }
        }

        public Task<IFileData> CreateFileAsync(string desiredName)
        {
            if (_abstractData is IFolderData folder)
                return folder.CreateFileAsync(desiredName);
            throw new NotSupportedException();
        }

        public Task<IFileData> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            if (_abstractData is IFolderData folder)
                return folder.CreateFileAsync(desiredName, options);
            throw new NotSupportedException();
        }

        public Task<IFolderData> CreateFolderAsync(string desiredName)
        {
            if (_abstractData is IFolderData folder)
                return folder.CreateFolderAsync(desiredName);
            throw new NotSupportedException();
        }

        public Task<IFolderData> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            if (_abstractData is IFolderData folder)
                return folder.CreateFolderAsync(desiredName, options);
            throw new NotSupportedException();
        }

        public Task DeleteAsync()
        {
            if (_abstractData is IFileData file)
                return file.Delete();
            else if (_abstractData is IFolderData folder)
                return folder.DeleteAsync();
            throw new NotSupportedException();
        }

        public Task EnsureExists()
        {
            if (_abstractData is IFolderData folder)
                return folder.EnsureExists();
            throw new NotSupportedException();
        }

        public Task<IFileData> GetFileAsync(string name)
        {
            if (_abstractData is IFolderData folder)
                return folder.GetFileAsync(name);
            throw new NotSupportedException();
        }

        public Task<IEnumerable<IFileData>> GetFilesAsync()
        {
            if (_abstractData is IFolderData folder)
                return folder.GetFilesAsync();
            throw new NotSupportedException();
        }

        public Task<IFolderData> GetFolderAsync(string name)
        {
            if (_abstractData is IFolderData folder)
                return folder.GetFolderAsync(name);
            throw new NotSupportedException();
        }

        public Task<IEnumerable<IFolderData>> GetFoldersAsync()
        {
            if (_abstractData is IFolderData folder)
                return folder.GetFoldersAsync();
            throw new NotSupportedException();
        }

        public Task<IFolderData> GetParentAsync()
        {
            if (_abstractData is IFileData file)
                return file.GetParentAsync();
            else if (_abstractData is IFolderData folder)
                return folder.GetParentAsync();
            throw new NotSupportedException();
        }

        Task IFileData.Delete() => DeleteAsync();

        public Task<Stream> GetStreamAsync(FileAccessMode accessMode = FileAccessMode.Read)
        {
            if (_abstractData is IFileData file)
                return file.GetStreamAsync(accessMode);
            throw new NotSupportedException();
        }

        public Task<Stream> GetThumbnailAsync(ThumbnailMode thumbnailMode, uint requiredSize)
        {
            if (_abstractData is IFileData file)
                return file.GetThumbnailAsync(thumbnailMode, requiredSize);
            throw new NotSupportedException();
        }

        public Task WriteAllBytesAsync(byte[] bytes)
        {
            if (_abstractData is IFileData file)
                return file.WriteAllBytesAsync(bytes);
            throw new NotSupportedException();
        }

        async Task<IMutableFileData> IMutableFileData.RenameAsync(string newFilename) => (await RenameAsync(newFilename)).AsMutableFile();

        async Task<IMutableFileData> IMutableFileData.MoveAsync(IFolderData destination) => (await MoveAsync(destination)).AsMutableFile();

        async Task<IMutableFileData> IMutableFileData.CopyAsync(IFolderData destination) => (await CopyAsync(destination)).AsMutableFile();

        async Task<IMutableFileData> IMutableFileData.MoveAndRenameAsync(IFolderData destination, string newName)
             => (await MoveAndRenameAsync(destination, newName)).AsMutableFile();

        async Task<IMutableFileData> IMutableFileData.CopyAndRenameAsync(IFolderData destination, string newName)
             => (await CopyAndRenameAsync(destination, newName)).AsMutableFile();

        public async Task<AbstractFileItemData> RenameAsync(string newName)
        {
            if (_abstractData is IMutableFileData file)
            {
                _abstractData = await file.RenameAsync(newName);
                return this;
            }
            throw new NotSupportedException();
        }

        public async Task<AbstractFileItemData> MoveAsync(IFolderData destination)
        {
            if (_abstractData is IMutableFileData file)
            {
                _abstractData = await file.MoveAsync(destination);
                return this;
            }
            throw new NotSupportedException();
        }

        public async Task<AbstractFileItemData> CopyAsync(IFolderData destination)
        {
            if (_abstractData is IMutableFileData file)
                return new(await file.CopyAsync(destination));
            throw new NotSupportedException();
        }

        public async Task<AbstractFileItemData> MoveAndRenameAsync(IFolderData destination, string newName)
        {
            if (_abstractData is IMutableFileData file)
            {
                _abstractData = await file.MoveAndRenameAsync(destination, newName);
                return this;
            }
            throw new NotSupportedException();
        }

        public async Task<AbstractFileItemData> CopyAndRenameAsync(IFolderData destination, string newName)
        {
            if (_abstractData is IMutableFileData file)
                return new(await file.CopyAndRenameAsync(destination, newName));
            throw new NotSupportedException();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IFileData AsFile() => _abstractData as IFileData;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IMutableFileData AsMutableFile() => _abstractData as IMutableFileData;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IFolderData AsFolder() => _abstractData as IFolderData;
    }
}
