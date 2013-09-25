
using Windows.Storage.Streams;
using Nokia.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System;
using Microsoft.Xna.Framework.Media;
using Windows.Foundation;
using System.Threading.Tasks;
//using System.IO.IsolatedStorage;
using Windows.Storage;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Growthstories.Domain.Messaging;

namespace Growthstories.UI.WindowsPhone
{
    public static class ImagingExtensions
    {
        public const string URI_SEPARATOR = "/";
        public const string IMG_FOLDER = @"LocalImages";
        public const string URI_SCHEME = @"ms-appdata://"; // Local folder URI scheme (WP8)

        public static IBuffer ToBuffer(this Stream stream)
        {
            var memoryStream = stream as MemoryStream;

            if (memoryStream == null)
            {
                using (memoryStream = new MemoryStream())
                {
                    stream.Position = 0;
                    stream.CopyTo(memoryStream);

                    try
                    {
                        // Some stream types do not support flushing

                        stream.Flush();
                    }
                    catch
                    {
                    }

                    return memoryStream.GetWindowsRuntimeBuffer();
                }
            }
            else
            {
                return memoryStream.GetWindowsRuntimeBuffer();
            }
        }



        public static PhotoOrientation Orientation(this IBuffer imageBuffer)
        {
            using (var session = new EditingSession(imageBuffer))
            {
                return session.Dimensions.Width >= session.Dimensions.Height ? PhotoOrientation.LANDSCAPE : PhotoOrientation.PORTRAIT;
            }
        }

        public static Task<Photo> SavePhotoToLocalStorageAsync(this Stream image)
        {
            return image.ToBuffer().SavePhotoToLocalStorageAsync();
            //var path = await buffer.SaveAsync();
            //var o = buffer.Orientation();
            //image.Position = 0;
            //return Tuple.Create(path, o);
        }

        /// <summary>
        /// Asynchronously saves a low resolution version of given photo to MediaLibrary. If the photo is too
        /// large to be saved to MediaLibrary as is, also saves the original high resolution photo to application's
        /// local storage so that the high resolution version is not lost.
        /// <param name="image">Photo to save</param>
        /// <returns>Path to the saved file in MediaLibrary</returns>
        /// </summary>
        public static async Task<Photo> SavePhotoToLocalStorageAsync(this IBuffer image)
        {
            //string savedPath = null;

            var photo = default(Photo);
            if (image != null && image.Length > 0)
            {

                //var filenameBase = "gsphoto_" + DateTime.UtcNow.Ticks.ToString();
                //savedPath = ,IMG_FOLDER + @"\" + filenameBase + @".jpg";
                photo.FileName = "gsphoto_" + DateTime.UtcNow.Ticks.ToString() + ".jpg";
                photo.LocalUri = URI_SCHEME + URI_SEPARATOR + IMG_FOLDER + URI_SEPARATOR + photo.FileName;

                uint maxBytes = (uint)(1.5 * 1024 * 1024); // 0.5 megabytes
                int maxPixels = (int)(1.25 * 1024 * 1024); // 1.25 megapixels
                var maxSize = new Size(4096, 4096); // Maximum texture size on WP8 is 4096x4096

                AutoResizeConfiguration resizeConfiguration = null;

                double w, h;
                uint s;

                using (var editingSession = new EditingSession(image))
                {
                    w = editingSession.Dimensions.Width;
                    h = (uint)editingSession.Dimensions.Height;
                    s = image.Length;

                    if (editingSession.Dimensions.Width * editingSession.Dimensions.Height > maxPixels)
                    {
                        var compactedSize = CalculateSize(editingSession.Dimensions, maxSize, maxPixels);

                        resizeConfiguration = new AutoResizeConfiguration(maxBytes, compactedSize,
                            new Size(0, 0), AutoResizeMode.Automatic, 0, ColorSpace.Yuv420);

                        w = compactedSize.Width;
                        h = compactedSize.Height;

                    }
                }

                if (resizeConfiguration != null)
                {
                    image = await Nokia.Graphics.Imaging.JpegTools.AutoResizeAsync(image, resizeConfiguration);
                    s = image.Length;
                }

                photo.Width = (uint)w;
                photo.Height = (uint)h;
                photo.Size = s;

                //var local = ;
                var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);
                var imgFile = await imgFolder.CreateFileAsync(photo.FileName, CreationCollisionOption.ReplaceExisting);
                //file.
                using (var writeStream = await imgFile.OpenStreamForWriteAsync())
                {
                    using (var readStream = image.AsStream())
                    {
                        await readStream.CopyToAsync(writeStream);
                        await writeStream.FlushAsync();
                    }
                }
                photo.LocalFullPath = imgFile.Path;

                //Windows.Storage.PathIO.


                //using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                //{

                //    if (!store.DirectoryExists(IMG_FOLDER))
                //    {
                //        store.CreateDirectory(IMG_FOLDER);
                //    }

                //    using (var file = store.CreateFile(savedPath))
                //    {
                //        using (var localImage = image.AsStream())
                //        {
                //            localImage.CopyTo(file);

                //            file.Flush();
                //        }
                //    }
                //}
            }

            return photo;
        }


        /// <summary>
        /// Calculates a new size from originalSize so that the maximum area is maxArea
        /// and maximum size is maxSize. Aspect ratio is preserved.
        /// </summary>
        /// <param name="originalSize">Original size</param>
        /// <param name="maxArea">Maximum area</param>
        /// <param name="maxSize">Maximum size</param>
        /// <returns>Area in same aspect ratio fits the limits set in maxArea and maxSize</returns>
        private static Size CalculateSize(Size originalSize, Size maxSize, double maxArea)
        {
            // Make sure that the image does not exceed the maximum size

            var width = originalSize.Width;
            var height = originalSize.Height;

            if (width > maxSize.Width)
            {
                var scale = maxSize.Width / width;

                width = width * scale;
                height = height * scale;
            }

            if (height > maxSize.Height)
            {
                var scale = maxSize.Height / height;

                width = width * scale;
                height = height * scale;
            }

            // Make sure that the image does not exceed the maximum area

            var originalPixels = width * height;

            if (originalPixels > maxArea)
            {
                var scale = Math.Sqrt(maxArea / originalPixels);

                width = originalSize.Width * scale;
                height = originalSize.Height * scale;
            }

            return new Size(width, height);
        }


        /// <summary>
        /// Takes a MediaLibrary photo path and tries to find a local high resolution copy of the same photo.
        /// </summary>
        /// <param name="libraryPath">Path to a photo in MediaLibrary</param>
        /// <returns>Stream to a local copy of the same photo</returns>
        //public static Stream LocalPhotoFromLibraryPath(this string libraryPath)
        //{
        //    var localPathCandidate = IMG_FOLDER + @"\" + FilenameFromPath(libraryPath);
        //    return OpenLocalPhoto(localPathCandidate);

        //}

        /// <summary>
        /// Takes a MediaLibrary photo path and tries to find a local high resolution copy of the same photo.
        /// </summary>
        /// <param name="libraryPath">Path to a photo in MediaLibrary</param>
        /// <returns>Stream to a local copy of the same photo</returns>
        //public static Stream OpenLocalPhoto(this string localPathCandidate)
        //{

        //    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        if (store.FileExists(localPathCandidate))
        //        {
        //            return store.OpenFile(localPathCandidate, FileMode.Open);
        //        }
        //    }

        //    return null;
        //}

        /// <summary>
        /// Takes a local high resolution photo path and tries to find MediaLibrary copy of the same photo.
        /// </summary>
        /// <param name="localPath">Path to a locally saved photo</param>
        /// <returns>Stream to a MediaLibrary copy of the same photo</returns>
        public static Stream LibraryPhotoFromLocalPath(this string localPath)
        {
            var localFilename = FilenameFromPath(localPath);

            using (var library = new MediaLibrary())
            {
                using (var pictures = library.Pictures)
                {
                    for (int i = 0; i < pictures.Count; i++)
                    {
                        using (var picture = pictures[i])
                        {
                            var libraryFilename = FilenameFromPath(picture.GetPath());

                            if (localFilename == libraryFilename)
                            {
                                return picture.GetImage();
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Takes a full path to a file and returns the last path component.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Last component of the given path</returns>
        private static string FilenameFromPath(string path)
        {
            var pathParts = path.Split('\\');
            return pathParts[pathParts.Length - 1];
        }


    }
}
