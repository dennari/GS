
using System;
using System.IO;
using System.Threading.Tasks;
using Growthstories.Core;
using Growthstories.Sync;

namespace Growthstories.UI.WindowsPhone
{
    public static class ImagingExtensions
    {

        public static IPhotoHandler Handler { get; set; }

        /// <summary>
        /// Asynchronously saves a low resolution version of given photo to MediaLibrary. If the photo is too
        /// large to be saved to MediaLibrary as is, also saves the original high resolution photo to application's
        /// local storage so that the high resolution version is not lost.
        /// <param name="image">Photo to save</param>
        /// <returns>Path to the saved file in MediaLibrary</returns>
        /// </summary>
        public static async Task<Photo> SavePhotoToLocalStorageAsync(this Stream image)
        {
            //string savedPath = null;

            var photo = new Photo();
            if (image != null)
            {


                Tuple<Stream, Size> scaled = null;

                try
                {
                    try
                    {
                        scaled = Handler.Scale(image);
                    }
                    catch // throws for example if photo's dimensions can't be read
                    {
                        return null;
                    }

                    photo.FileName = Handler.GeneratePhotoFilename();
                    photo.LocalUri = Handler.GetPhotoLocalUri(photo.FileName);
                    photo.Width = (uint)scaled.Item2.Width;
                    photo.Height = (uint)scaled.Item2.Height;

                    Stream img = scaled.Item1;
                    img.Position = 0;

                    photo.LocalFullPath = await Handler.WriteToDisk(img, photo.FileName);
                }
                finally
                {
                    if (scaled != null && scaled.Item1 != null)
                    {
                        scaled.Item1.Close();
                    }
                }
            }
            return photo;
        }









    }
}
