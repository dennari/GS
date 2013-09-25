
using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
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

namespace Growthstories.UI.WindowsPhone
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


            BitmapImage img = null;
            if (p != null)
            {
                if (p.StartsWith(@"\"))
                {
                    //Stream s = p.OpenLocalPhoto();
                    //if (s == null)
                    //    return null;

                    //Uri uri = new Uri(path, path.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);
                    //img = new BitmapImage();
                    //img.UriSource = new Uri(p, p.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);
                    //img.SetSource(s);
                }
                else
                {
                    img = new BitmapImage();
                    img.UriSource = new Uri(p, p.StartsWith("/") ? UriKind.Relative : UriKind.Absolute);

                }

            }

            Uri u = value as Uri;
            if (img == null && u != null)
            {
                img = new BitmapImage();
                img.UriSource = u;
            }


            if (img != null)
            {
                img.ImageFailed += img_ImageFailed;
                img.ImageOpened += img_ImageOpened;
            }


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
                    DecodePixelType = DecodePixelType.Physical,
                    DecodePixelHeight = (int)x.Height,
                    DecodePixelWidth = (int)x.Width
                };
                img.ImageFailed += img_ImageFailed;
                return img;
            }
            catch (InvalidCastException)
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






    public class AppBarModeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string ret = "Default";
            try
            {
                var p = (ApplicationBarMode)value;
                if (p == ApplicationBarMode.MINIMIZED)
                    ret = "Minimized";
            }
            catch
            {

            }
            return ret;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Behavior that will connect an UI event to a viewmodel Command,
    /// allowing the event arguments to be passed as the CommandParameter.
    /// </summary>
    public class EventToCommandBehavior : Behavior<FrameworkElement>
    {
        private Delegate _handler;
        private EventInfo _oldEvent;

        // Event
        public string Event { get { return (string)GetValue(EventProperty); } set { SetValue(EventProperty, value); } }
        public static readonly DependencyProperty EventProperty = DependencyProperty.Register("Event", typeof(string), typeof(EventToCommandBehavior), new PropertyMetadata(null, OnEventChanged));

        // Command
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommandBehavior), new PropertyMetadata(null));

        // PassArguments (default: false)
        public bool PassArguments { get { return (bool)GetValue(PassArgumentsProperty); } set { SetValue(PassArgumentsProperty, value); } }
        public static readonly DependencyProperty PassArgumentsProperty = DependencyProperty.Register("PassArguments", typeof(bool), typeof(EventToCommandBehavior), new PropertyMetadata(false));


        private static void OnEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var beh = (EventToCommandBehavior)d;

            if (beh.AssociatedObject != null) // is not yet attached at initial load
                beh.AttachHandler((string)e.NewValue);
        }

        protected override void OnAttached()
        {
            AttachHandler(this.Event); // initial set
        }

        /// <summary>
        /// Attaches the handler to the event
        /// </summary>
        private void AttachHandler(string eventName)
        {
            // detach old event
            if (_oldEvent != null)
                _oldEvent.RemoveEventHandler(this.AssociatedObject, _handler);

            // attach new event
            if (!string.IsNullOrEmpty(eventName))
            {
                EventInfo ei = this.AssociatedObject.GetType().GetEvent(eventName);
                if (ei != null)
                {
                    MethodInfo mi = this.GetType().GetMethod("ExecuteCommand", BindingFlags.Instance | BindingFlags.NonPublic);
                    _handler = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                    ei.AddEventHandler(this.AssociatedObject, _handler);
                    _oldEvent = ei; // store to detach in case the Event property changes
                }
                else
                    throw new ArgumentException(string.Format("The event '{0}' was not found on type '{1}'", eventName, this.AssociatedObject.GetType().Name));
            }
        }

        /// <summary>
        /// Executes the Command
        /// </summary>
        private void ExecuteCommand(object sender, EventArgs e)
        {
            object parameter = this.PassArguments ? e : null;
            if (this.Command != null)
            {
                if (this.Command.CanExecute(parameter))
                    this.Command.Execute(parameter);
            }
        }
    }

}