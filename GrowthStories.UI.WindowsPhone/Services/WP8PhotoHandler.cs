
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using EventStore.Logging;
using Windows.Storage;

using Size = Growthstories.Core.Size;

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


        Size maxSize = new Size(2000, 2000); // some limit specified in the documentation
        double maxArea = double.MaxValue; // we don't want to limit by area

        public WP8PhotoHandler()
        {
            //var memLimit = DeviceStatus.ApplicationMemoryUsageLimit / 1024 / 1024; // in MB

            maxSize = ResolutionHelper.MaxImageSize;
        }


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
                .Replace("+", "_").Replace("/", "-").Replace("=", "");

            return temp + ".jpg";
        }


        /// <summary>
        /// Calculates a new size from originalSize so that the maximum area is maxArea
        /// and maximum size is maxSize. Aspect ratio is preserved.
        /// </summary>
        /// <param name="originalSize">Original size</param>
        /// <param name="maxArea">Maximum area</param>
        /// <param name="maxSize">Maximum size</param>
        /// <returns>Area in same aspect ratio fits the limits set in maxArea and maxSize</returns>

        public Size CalculateSize(Size originalSize)
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


        public Tuple<Stream, Size> Scale(Stream original)
        {

            BitmapImage img = new BitmapImage();
            img.CreateOptions = BitmapCreateOptions.DelayCreation;
            img.SetSource(original);

            var size = new Size(img.PixelWidth, img.PixelHeight);

            var saveSize = CalculateSize(size);

            WriteableBitmap wBitmap = new WriteableBitmap(img);
            MemoryStream ms = new MemoryStream();
            wBitmap.SaveJpeg(ms, (int)saveSize.Width, (int)saveSize.Height, 0, JpegQuality);

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


        public int JpegQuality
        {
            get { return 80; }
        }
    }


    public enum Resolutions { WVGA, WXGA, HD };

    public static class ResolutionHelper
    {
        private static bool IsWvga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 100;
            }
        }

        private static bool IsWxga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 160;
            }
        }

        private static bool IsHD
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 150;
            }
        }

        public static Resolutions CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolutions.WVGA;
                else if (IsWxga) return Resolutions.WXGA;
                else if (IsHD) return Resolutions.HD;
                else throw new InvalidOperationException("Unknown resolution");
            }
        }

        public static Size MaxImageSize
        {
            get
            {
                try
                {
                    var resolution = CurrentResolution;
                    if (resolution == Resolutions.WXGA)
                    {
                        return new Size(1280, 1280);
                    }
                    if (resolution == Resolutions.HD)
                    {
                        return new Size(1280, 1280);
                    }
                }
                catch (InvalidOperationException) { }

                return new Size(800, 800);
            }
        }

    }

}
