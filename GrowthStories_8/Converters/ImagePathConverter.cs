using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.Windows.Data;
using Growthstories.WP8.Domain.Entities;

namespace Growthstories.WP8.Converters
{

    /// <summary>
    /// Each picture is stored as a byte array in the PictureViewModel object.
    /// When we bind to that property we must convert to an image source that can be used by the Image control.
    /// </summary>
    public class ImagePathConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return this.Convert(value, targetType, parameter, culture.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        private object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value == null) return null;

            Plant p = value as Plant;
            //Uri uri = new Uri(path, path.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);
            BitmapImage img = new BitmapImage();
            //img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            //img.CreateOptions = BitmapCreateOptions.BackgroundCreation;
            if (p.ProfilePicture == null)
            {

                img.UriSource = new Uri(p.ProfilepicturePath, p.ProfilepicturePath.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);
            }
            else
            {
                img.SetSource(p.ProfilePicture);
            }
            img.ImageFailed += img_ImageFailed;
            img.ImageOpened += img_ImageOpened;

            return img;
        }

        void img_ImageOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (true) { }
        }

        void img_ImageFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            throw e.ErrorException;
        }

    }

    public class StreamToBmp : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            if (value == null)
            {
                return null;
            }
            var bmp = new System.Windows.Media.Imaging.BitmapImage();
            bmp.SetSource(value as Stream);
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }

}
