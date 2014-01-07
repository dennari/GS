
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

        //public const string IMG_FOLDER = @"LocalImages";
        public const string IMG_FOLDER = @"LocalImages";

        public const string URI_SEPARATOR = "/";
        //public const string URI_SCHEME = @"ms-appdata://"; // Local folder URI scheme (WP8)

        public const string URI_SCHEME = @"isostore:"; // Local folder URI scheme (WP8)


        public static async Task<StorageFolder> GetImageFolder()
        {
            var shared = await ApplicationData.Current.LocalFolder.GetFolderAsync("Shared");
            var ret = await shared.GetFolderAsync("ShellContent");
            
            return ret;
        }


        public static string GetImagePath(string filename)
        {
            return URI_SCHEME + URI_SEPARATOR + @"Shared" + URI_SEPARATOR + @"ShellContent" + URI_SEPARATOR + filename;
        }


        public async Task<Stream> ReadPhoto(Photo photo)
        {
            //var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);
            var imgFolder = await GetImageFolder();

            return await imgFolder.OpenStreamForReadAsync(photo.FileName);
        }

        
        public async Task<Stream> WritePhoto(Photo photo)
        {
            //var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);

            var imgFolder = await GetImageFolder();
            return await imgFolder.OpenStreamForWriteAsync(photo.FileName, CreationCollisionOption.ReplaceExisting);
        }

    }
}
