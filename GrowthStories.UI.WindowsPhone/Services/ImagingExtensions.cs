
using Growthstories.Sync;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Foundation;

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

                uint maxBytes = (uint)(1.5 * 800 * 800); // 0.5 megabytes
                int maxPixels = (int)(1.0 * 800 * 800); // 1.25 megapixels
                var maxSize = new Size(800, 800); // Maximum texture size on WP8 is 4096x4096

                //uint maxBytes = (uint)(1.5 * 1024 * 1024); // 0.5 megabytes
                //int maxPixels = (int)(1.25 * 1024 * 1024); // 1.25 megapixels
                //var maxSize = new Size(4096, 4096); // Maximum texture size on WP8 is 4096x4096

                Tuple<Stream, Size> scaled = null;

                try
                {
                    try
                    {
                        scaled = image.Scale(maxPixels, maxSize, maxBytes);
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


        private static Tuple<Stream, Size> Scale(this Stream image, int maxPixels, Size maxSize, uint maxBytes)
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



    }
}
