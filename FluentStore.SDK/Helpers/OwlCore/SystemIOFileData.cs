using System;
using System.IO;
using System.Threading.Tasks;

namespace OwlCore.AbstractStorage
{
    /// <inheritdoc cref="IFileData"/>
    public class SystemIOFileData : IFileData
    {
        /// <summary>
        /// The underlying <see cref="File"/> instance in use.
        /// </summary>
        internal FileInfo File { get; }

        /// <summary>
        /// Creates a new instance of <see cref="SystemIOFileData" />.
        /// </summary>
        /// <param name="file">The <see cref="FileInfo"/> to wrap.</param>
        public SystemIOFileData(FileInfo file)
        {
            File = file;
            Properties = new FileDataProperties(file);
        }

        /// <summary>
        /// Creates a new instance of <see cref="SystemIOFileData" />.
        /// </summary>
        /// <param name="file">The path of the file to wrap.</param>
        public SystemIOFileData(string filePath) : this(new FileInfo(filePath))
        {

        }

        /// <inheritdoc/>
        public string Path => File.FullName;

        /// <inheritdoc/>
        public string Name => File.Name;

        /// <inheritdoc/>
        public string DisplayName => File.Name;

        /// <inheritdoc/>
        public string FileExtension => File.Extension;

        /// <inheritdoc/>
        public string Id => FluentStore.SDK.Helpers.StorageHelper.GetFileId(File);

        /// <inheritdoc/>
        public IFileDataProperties Properties { get; set; }

        /// <inheritdoc/>
        public Task<IFolderData> GetParentAsync()
        {
            return Task.FromResult<IFolderData>(new SystemIOFolderData(File.Directory));
        }

        /// <inheritdoc/>
        public Task Delete()
        {
            return Task.Run(File.Delete);
        }

        /// <inheritdoc />
        public Task<Stream> GetStreamAsync(FileAccessMode accessMode = FileAccessMode.Read)
        {
            return Task.Run<Stream>(() => File.Open(
                FileMode.Open,
                accessMode == FileAccessMode.ReadWrite ? FileAccess.ReadWrite : FileAccess.Read));
        }

        /// <inheritdoc />
        public Task WriteAllBytesAsync(byte[] bytes)
        {
            return System.IO.File.WriteAllBytesAsync(File.FullName, bytes);
        }

        /// <inheritdoc />
        public async Task<Stream> GetThumbnailAsync(ThumbnailMode thumbnailMode, uint requiredSize)
        {
            throw new NotImplementedException();
        }
    }

}
