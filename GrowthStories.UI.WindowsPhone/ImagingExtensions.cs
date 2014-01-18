﻿
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
using Growthstories.Sync;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

namespace Growthstories.UI.WindowsPhone
{
    public static class ImagingExtensions
    {

        public const string URI_SEPARATOR = "/";
        public const string IMG_FOLDER = WP8PhotoHandler.IMG_FOLDER;
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



        public static async Task<PhotoOrientation> Orientation(this Stream image)
        {
            using (var source = new StreamImageSource(image, ImageFormat.Jpeg))
            {
                var info = await source.GetInfoAsync();

                return info.ImageSize.Width >= info.ImageSize.Height ? PhotoOrientation.LANDSCAPE : PhotoOrientation.PORTRAIT;
            }
        }

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

                uint maxBytes = (uint)(1.5 * 1024 * 1024); // 0.5 megabytes
                int maxPixels = (int)(1.25 * 1024 * 1024); // 1.25 megapixels
                var maxSize = new Size(4096, 4096); // Maximum texture size on WP8 is 4096x4096

                Tuple<Stream, Size> scaled = null;
                try
                {
                    scaled = await image.ScaleAsync(maxPixels, maxSize, maxBytes);
                }
                catch // throws for example if photo's dimensions can't be read
                {
                    return null;
                }

                //var filenameBase = "gsphoto_" + DateTime.UtcNow.Ticks.ToString();
                //savedPath = ,IMG_FOLDER + @"\" + filenameBase + @".jpg";
                photo.FileName = "gsphoto_" + DateTime.UtcNow.Ticks.ToString() + ".jpg";
                photo.LocalUri = WP8PhotoHandler.GetImagePath(photo.FileName);
                //photo.LocalUri = URI_SCHEME + URI_SEPARATOR + IMG_FOLDER + URI_SEPARATOR + photo.FileName;

                photo.Width = (uint)scaled.Item2.Width;
                photo.Height = (uint)scaled.Item2.Height;
                //photo.Size = s;

                //var local = ;
                //var imgFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(IMG_FOLDER, CreationCollisionOption.OpenIfExists);
                var imgFolder = await WP8PhotoHandler.GetImageFolder();

                var imgFile = await imgFolder.CreateFileAsync(photo.FileName, CreationCollisionOption.ReplaceExisting);
                //file.
                var img = scaled.Item1;
                //img.Seek(0, SeekOrigin.Begin);

                img.Position = 0;
                using (var writeStream = await imgFile.OpenStreamForWriteAsync())
                {
                    using (var readStream = img)
                    {
                        await readStream.CopyToAsync(writeStream);
                        await writeStream.FlushAsync();
                    }
                }
                photo.LocalFullPath = imgFile.Path;

            }
            return photo;
        }


        public static async Task<Tuple<Stream, Size>> ScaleAsync(this Stream image, int maxPixels, Size maxSize, uint maxBytes)
        {
    
            BitmapImage img = new BitmapImage();
            img.CreateOptions = BitmapCreateOptions.DelayCreation;
            img.SetSource(image);

            var size = new Size(img.PixelWidth, img.PixelHeight);

            var saveSize = size;
            if (size.Width * size.Height > maxPixels)
            {
                saveSize = CalculateSize(size, maxSize, maxPixels);    
            }

            WriteableBitmap wBitmap = new WriteableBitmap(img);
            MemoryStream ms = new MemoryStream();
            wBitmap.SaveJpeg(ms, (int)saveSize.Width, (int)saveSize.Height, 0, 100);
            return Tuple.Create((System.IO.Stream)ms, saveSize);


            //double w, h;
            //long s;

            //var info = new ImageProviderInfo();
            //info.ImageSize.Height = 20;

            //return new Tuple(ms, 

            //using (var source = new StreamImageSource(image, ImageFormat.Undefined))
            //{
            //    var info = await source.GetInfoAsync();
                
            //    w = info.ImageSize.Width;
            //    h = info.ImageSize.Height;
            //    s = image.Length;

            //    if (w * h > maxPixels && source.ImageFormat == ImageFormat.Jpeg)
            //    {
            //        var compactedSize = CalculateSize(info.ImageSize, maxSize, maxPixels);

            //        var resizeConfiguration = new AutoResizeConfiguration(maxBytes, compactedSize,
            //            new Size(0, 0), AutoResizeMode.Automatic, 0, ColorSpace.Yuv420);

            //        var buffer = await Nokia.Graphics.Imaging.JpegTools.AutoResizeAsync(image.ToBuffer(), resizeConfiguration);

            //        return Tuple.Create(buffer.AsStream(), compactedSize, source.ImageFormat);
                
            //    } else {
            //        return Tuple.Create(image, info.ImageSize, source.ImageFormat);
            //    }
            //}



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
