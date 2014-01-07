using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace Growthstories.WP8.Converters
{

    /// <summary>
    /// Each picture is stored as a byte array in the PictureViewModel object.
    /// When we bind to that property we must convert to an image source that can be used by the Image control.
    /// </summary>
    public class BitmapImageByteArrayConverter : IValueConverter
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

            return value == null ? null : GetBitmap((string)value);
        }

        private BitmapImage GetBitmap(byte[] bytes)
        {
            var bmp = new BitmapImage();

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                bmp.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                ms.Seek(0, SeekOrigin.Begin);
                bmp.SetSource(ms);
            };

            return bmp;
        }

        private BitmapImage GetBitmap(string path)
        {
            return new BitmapImage(new Uri(path));
        }
    }

}
