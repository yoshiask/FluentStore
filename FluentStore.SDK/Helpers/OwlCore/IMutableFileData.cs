using System.Threading.Tasks;

namespace OwlCore.AbstractStorage
{
    /// <summary>
    /// Represents a file that resides on a file system that can
    /// be renamed, moved, and copied.
    /// </summary>
    public interface IMutableFileData : IFileData
    {
        /// <summary>
        /// Renames the file.
        /// </summary>
        /// <param name="newFilename">
        /// The new name for the file.
        /// </param>
        public Task<IMutableFileData> RenameAsync(string newFilename);

        /// <summary>
        /// Moves the file.
        /// </summary>
        /// <param name="destination">
        /// The folder to move the file to.
        /// </param>
        public Task<IMutableFileData> MoveAsync(IFolderData destination);

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="destination">
        /// The folder to copy the file to.
        /// </param>
        public Task<IMutableFileData> CopyAsync(IFolderData destination);

        /// <summary>
        /// Moves and renames the file.
        /// </summary>
        /// <param name="destination">
        /// The folder to move the file to.
        /// </param>
        /// <param name="newName">
        /// The name for the moved file.
        /// </param>
        public Task<IMutableFileData> MoveAndRenameAsync(IFolderData destination, string newName);

        /// <summary>
        /// Copies and renames the file.
        /// </summary>
        /// <param name="destination">
        /// The folder to copy the file to.
        /// </param>
        /// <param name="newName">
        /// The name for the copied file.
        /// </param>
        public Task<IMutableFileData> CopyAndRenameAsync(IFolderData destination, string newName);
    }
}
