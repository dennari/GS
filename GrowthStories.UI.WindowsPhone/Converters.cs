
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GrowthStories.UI.WindowsPhone
{

    /// <summary>
    /// Each picture is stored as a byte array in the PictureViewModel object.
    /// When we bind to that property we must convert to an image source that can be used by the Image control.
    /// </summary>
    public class ActionToStringConverter : IValueConverter
    {

        private IDictionary<Type, string> ActionToString = new Dictionary<Type, string>()
        {
            {typeof(Watered),"Watered"},
            {typeof(Fertilized),"Fertilized"},
            {typeof(Commented),"Commented"}
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (targetType != typeof(string))
                throw new InvalidOperationException("Can only convert to string");

            if (!(value is ActionBase))
                return "Unknown value";

            return value.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class ActionToDateStringConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (targetType != typeof(string))
                throw new InvalidOperationException("Can only convert to string");

            var action = value as ActionBase;
            if (action == null)
                return "Unknown value";

            return action.Created.ToString("G", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

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

    /// <summary>
    /// Each picture is stored as a byte array in the PictureViewModel object.
    /// When we bind to that property we must convert to an image source that can be used by the Image control.
    /// </summary>
    public class PathToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            string p = value as string;
            if (p == null)
                return null;

            BitmapImage img = null;
            if (p.StartsWith(@"\"))
            {
                Stream s = p.OpenLocalPhoto();
                if (s == null)
                    return null;

                //Uri uri = new Uri(path, path.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);
                img = new BitmapImage();
                //img.UriSource = new Uri(p, p.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);
                img.SetSource(s);
            }
            else
            {
                img = new BitmapImage();
                img.UriSource = new Uri(p, p.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);

            }



            img.ImageFailed += img_ImageFailed;
            img.ImageOpened += img_ImageOpened;

            return img;
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