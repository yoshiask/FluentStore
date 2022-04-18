using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwlCore.AbstractStorage
{
    /// <inheritdoc />
    public class FileDataProperties : IFileDataProperties
    {
        private readonly FileInfo _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDataProperties"/> class.
        /// </summary>
        /// <param name="file">The file to get properties from.</param>
        public FileDataProperties(FileInfo file)
        {
            _file = file;
        }

        /// <inheritdoc />
        public async Task<MusicFileProperties?> GetMusicPropertiesAsync()
        {
            throw new NotImplementedException();
        }
    }
}