
using Windows.Storage.Streams;
using System.IO;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;


namespace Growthstories.Sync
{
    public sealed class WP8PhotoHandler : IPhotoHandler
    {

        public const string IMG_FOLDER = @"LocalImages";

        public async Task<Stream> ReadPhoto(Photo photo)
        {
            var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);
            return await imgFolder.OpenStreamForReadAsync(photo.FileName);
        }

        public async Task<Stream> WritePhoto(Photo photo)
        {
            var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);
            return await imgFolder.OpenStreamForWriteAsync(photo.FileName, CreationCollisionOption.ReplaceExisting);
        }

    }
}
