// https://github.com/Arlodotexe/strix-music/blob/e7d2faee69420b6b4c75ece36aa2cbbedb34facb/src/Sdk/StrixMusic.Sdk.WinUI/Models/FileDataProperties.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwlCore.AbstractStorage
{
    /// <inheritdoc />
    public class StorageFileDataProperties : IFileDataProperties
    {
        private readonly StorageFile _storageFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageFileDataProperties"/> class.
        /// </summary>
        /// <param name="storageFile">The storage file to get properties from.</param>
        public StorageFileDataProperties(StorageFile storageFile)
        {
            _storageFile = storageFile;
        }

        /// <inheritdoc />
        public async Task<MusicFileProperties?> GetMusicPropertiesAsync()
        {
            var storageFileMusicProps = await _storageFile.Properties.GetMusicPropertiesAsync();

            var musicFileProps = new MusicFileProperties()
            {
                Album = storageFileMusicProps.Album,
                AlbumArtist = storageFileMusicProps.AlbumArtist,
                Artist = storageFileMusicProps.Artist,
                Bitrate = storageFileMusicProps.Bitrate,
                Composers = storageFileMusicProps.Composers.ToArray(),
                Conductors = storageFileMusicProps.Conductors.ToArray(),
                Duration = storageFileMusicProps.Duration,
                Genres = storageFileMusicProps.Genre.ToArray(),
                Producers = storageFileMusicProps.Producers.ToArray(),
                Publisher = storageFileMusicProps.Publisher,
                Rating = storageFileMusicProps.Rating,
                Subtitle = storageFileMusicProps.Subtitle,
                Title = storageFileMusicProps.Title,
                TrackNumber = storageFileMusicProps.TrackNumber,
                Writers = storageFileMusicProps.Writers.ToArray(),
                Year = storageFileMusicProps.Year,
            };

            return musicFileProps;
        }
    }
}