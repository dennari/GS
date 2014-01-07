using Microsoft.Phone;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Growthstories.PCL.Services;

namespace Growthstories.WP8.Services
{
    /// <summary>
    /// Asynchronously get a picture using the specified path
    /// </summary>
    /// <param name="path">The path to the picture</param>
    /// <returns>The picture as a byte array on success; Otherwise, null</returns>
    /// <remarks>The path parameter in the Windows Phone implementation is not a file path. Instead, it is 
    /// a combination of the album name and the picture name.</remarks>
    public class PictureService : IPictureService
    {
        public Task<byte[]> GetImageBytesAsync(string path)
        {
            return Task.Run(delegate
            {
                Debug.WriteLine("PicturePath '{0}'", path);

                // Extract the album name and picture name from the path
                var pathParts = path.Split("|".ToCharArray());
                string albumName = pathParts[0];
                string pictureName = pathParts[1];

                byte[] result = null;

                MediaLibrary mediaLib = new MediaLibrary();
                foreach (var album in mediaLib.RootPictureAlbum.Albums)
                {
                    // Find the album
                    if (album.Name == albumName)
                    {
                        foreach (var pic in album.Pictures)
                        {
                            if (pic.Name == pictureName)
                            {
                                using (var picStream = pic.GetImage())
                                {
                                    using (BinaryReader br = new BinaryReader(picStream))
                                    {
                                        Debug.WriteLine("picStream.Length {0}", picStream.Length);
                                        result = br.ReadBytes((int)picStream.Length);
                                    }
                                }
                            }
                        }
                    }
                }
                Debug.WriteLine(result.Length);

                return result;
            });
        }


    }
}
