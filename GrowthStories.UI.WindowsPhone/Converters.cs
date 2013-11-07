
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Entities;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using System;
using System.Linq;
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

    public static class ConverterHelpers
    {
        public static BitmapImage ToBitmapImage(this Photo x)
        {
            var img = new BitmapImage(new Uri(x.Uri, UriKind.RelativeOrAbsolute))
              {
                  CreateOptions = BitmapCreateOptions.DelayCreation,
                  DecodePixelType = DecodePixelType.Physical
              };
            if (x.Height != default(uint))
                img.DecodePixelHeight = (int)x.Height;
            if (x.Width != default(uint))
                img.DecodePixelWidth = (int)x.Width;
            img.ImageFailed += (s, e) =>
            {
                throw e.ErrorException;
            };
            return img;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            //return value == null ? Visibility.Collapsed : Visibility.Visible;
            //if (targetType != typeof(Visibility))
            //    throw new InvalidOperationException("Can only convert to Visibility");


            if (parameter == null)
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            else
                return value != null ? Visibility.Collapsed : Visibility.Visible;

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
                return x.ToBitmapImage();
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


    }


    public class PlantToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                IPlantViewModel x = (IPlantViewModel)value;
                if (x.Photo != null)
                    return x.Photo.ToBitmapImage();


                var defaultPhoto = new Photo()
                {
                    LocalUri = "/Assets/Tiles/IconImage_03.png",
                    LocalFullPath = "/Assets/Tiles/IconImage_03.png",
                    Width = 134,
                    Height = 202
                };

                return defaultPhoto.ToBitmapImage();

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


    public class PlantActionFilter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
                return null;

            var v = value as IEnumerable<IPlantActionViewModel>;
            if (v == null)
                return null;
            if (parameter != null)
            {
                var t = parameter as string;
                if (t != null)
                {
                    if (t == "Comment")
                        return v.Where(x => x.ActionType == PlantActionType.COMMENTED);
                }
            }


            return v.Where(x => x.ActionType == PlantActionType.PHOTOGRAPHED);

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