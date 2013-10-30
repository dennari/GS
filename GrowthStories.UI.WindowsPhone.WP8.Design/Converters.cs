
//using Growthstories.Domain.Messaging;
using Growthstories.Sync;
//using Growthstories.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace Growthstories.UI.WindowsPhone.Design
{


    //}

    public class NullToVisibilityConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("Can only convert to Visibility");

            var v = value as string;
            if (parameter == null)
                return string.IsNullOrWhiteSpace(v) ? Visibility.Visible : Visibility.Collapsed;
            else
                return string.IsNullOrWhiteSpace(v) ? Visibility.Collapsed : Visibility.Visible;

        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }


    public class UriToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            return new BitmapImage(value as Uri);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }


    public class PhotoToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                Photo x = (Photo)value;
                var img = new BitmapImage(new Uri(x.Uri, UriKind.RelativeOrAbsolute))
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelType = DecodePixelType.Physical
                };
                if (x.Height != default(uint))
                    img.DecodePixelHeight = (int)x.Height;
                if (x.Width != default(uint))
                    img.DecodePixelWidth = (int)x.Width;
                img.ImageFailed += img_ImageFailed;
                return img;
            }
            catch (InvalidCastException)
            {

            }
            catch (ArgumentNullException)
            {

            }
            return null;


        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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


}