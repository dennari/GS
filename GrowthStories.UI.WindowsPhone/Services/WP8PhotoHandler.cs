
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
using PWDTK_MOBILE_WP_8;


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

            return await imgFolder.OpenStreamForReadAsync(filename);
        }


        public async Task<Tuple<Stream, string>> OpenWriteStream(string filename)
        {
            //var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);

            var imgFolder = await GetImageFolder();
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



        public string FilenameFromBlobKey(string blobKey)
        {

            var salt = "GSCRAZYSALTWHOA";

            string t = PWDTK.HashBytesToHexString(PWDTK.PasswordToHash(Encoding.UTF8.GetBytes(salt), blobKey));

            return t.Substring(0, 32) + ".jpg";


        }
    }
}
