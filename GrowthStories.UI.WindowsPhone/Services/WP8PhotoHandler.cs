
using Windows.Storage.Streams;
using System.IO;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Windows.Foundation;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using EventStore.Logging;


namespace Growthstories.Sync
{
    public sealed class WP8PhotoHandler : IPhotoHandler
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(WP8PhotoHandler));


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


        public string GetPhotoLocalUri(string filename)
        {
            return URI_SCHEME + URI_SEPARATOR + @"Shared" + URI_SEPARATOR + @"ShellContent" + URI_SEPARATOR + filename;
        }

        public string GeneratePhotoFilename(string extension = "jpg")
        {
            return "gsphoto_" + DateTime.UtcNow.Ticks.ToString() + "." + extension;
        }


        public async Task<Stream> OpenReadStream(string filename)
        {
            //var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);
            var imgFolder = await GetImageFolder();

            Logger.Info("opening read stream for filename {0}, path {1}", filename, imgFolder.Path);

            return await imgFolder.OpenStreamForReadAsync(filename);
        }


        public async Task<Tuple<Stream, string>> OpenWriteStream(string filename)
        {
            //var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);

            var imgFolder = await GetImageFolder();

            Logger.Info("opening write stream for filename {0}, path {1}", filename, imgFolder.Path);

            var imgFile = await imgFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            var stream = await imgFile.OpenStreamForWriteAsync();
            var path = imgFile.Path;

            return Tuple.Create(stream, path);
        }


        public async Task<string> WriteToDisk(Stream readStream, string filename)
        {
            var r = await OpenWriteStream(filename);
            using (var writeStream = r.Item1)
            {
                using (readStream)
                {
                    await readStream.CopyToAsync(writeStream);
                    await writeStream.FlushAsync();
                }
            }
            return r.Item2;
        }


        private SHA1Managed _SHA;
        private SHA1Managed SHA
        {
            get
            {
                return _SHA ?? (_SHA = new SHA1Managed());
            }
        }


        public string FilenameFromBlobKey(string blobKey)
        {
            var temp = Convert.ToBase64String(SHA.ComputeHash(Encoding.UTF8.GetBytes(blobKey)))
                .Replace("+", "_").Replace("/", "-").Replace("=","");

            return temp + ".jpg";
        }

    }
}
