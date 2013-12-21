
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
        public static BitmapImage ToBitmapImage(this IPhoto x)
        {
            var img = new BitmapImage(new Uri(x.Uri, UriKind.RelativeOrAbsolute))
              {
                  CreateOptions = (BitmapCreateOptions.DelayCreation & BitmapCreateOptions.BackgroundCreation),
                  DecodePixelType = x.DimensionsType == DimensionsType.LOGICAL ? DecodePixelType.Logical : DecodePixelType.Physical
              };
            if (x.Height != default(uint))
                img.DecodePixelHeight = (int)x.Height;
            // the width is ignored when using BackGroundCreation
            //if (x.Width != default(uint)) 
            //    img.DecodePixelWidth = (int)x.Width;
            img.ImageFailed += (s, e) =>
            {
                //throw e.ErrorException;
            };
            return img;
        }


        public const string IconFolder = "/Assets/Icons/";

        public static IDictionary<IconType, string> SmallIcons = new Dictionary<IconType, string>()
        {
            {IconType.ADD,IconFolder+"appbar.add.png"},
            {IconType.CHECK,IconFolder+"appbar.check.png"},
            {IconType.DELETE,IconFolder+"appbar.delete.png"},
            {IconType.CANCEL,IconFolder+"ApplicationBar.Cancel.png"},
            {IconType.CHECK_LIST,IconFolder+"appbar.list.check.png"},
            {IconType.SHARE,IconFolder+"appbar.social.sharethis.png"},
            {IconType.WATER,IconFolder+"icon_watering_appbar.png"},
            {IconType.PHOTO,IconFolder+"icon_photo_appbar.png"},
            {IconType.FERTILIZE,IconFolder+"icon_nutrient_appbar.png"},
            {IconType.NOURISH,IconFolder+"icon_nutrient_appbar.png"},
            {IconType.NOTE,IconFolder+"icon_comment_appbar.png"},
            {IconType.MEASURE,IconFolder+"icon_length_appbar.png"},
            {IconType.CHANGESOIL,IconFolder+"icon_soilchange_appbar.png"},
            {IconType.BLOOMING,IconFolder+"icon_blooming_appbar.png"},
            {IconType.DECEASED,IconFolder+"icon_deceased_appbar.png"},
            {IconType.ILLUMINANCE,IconFolder+"icon_illuminance_appbar.png"},
            {IconType.MISTING,IconFolder+"icon_misting_appbar.png"},
            {IconType.PH,IconFolder+"icon_ph_appbar.png"},
            {IconType.POLLINATION,IconFolder+"icon_pollination_appbar.png"},
            {IconType.SPROUTING,IconFolder+"icon_sprouting_appbar.png"},
            {IconType.PH2,IconFolder+"icon_ph2_appbar.png"},
            {IconType.CO2,IconFolder+"icon_co2_appbar.png"},
            {IconType.AIRHUMIDITY,IconFolder+"icon_air_humidity_appbar.png"},
            {IconType.SETTINGS,IconFolder+"appbar.settings.png"},
            {IconType.SIGNIN,IconFolder+"appbar.door.enter.png"},
            {IconType.SIGNOUT,IconFolder+"appbar.door.leave.png"}

        };




        public static IDictionary<IconType, string> BigIcons = new Dictionary<IconType, string>()
        {
            {IconType.WATER, "/Assets/Icons/icon_watering.png"},
            {IconType.PHOTO, "/Assets/Icons/icon_photo.png"},
            {IconType.FERTILIZE,"/Assets/Icons/icon_nutrient.png"},
            {IconType.NOURISH,"/Assets/Icons/icon_nutrient.png"},
            {IconType.NOTE,"/Assets/Icons/icon_comment.png"},
            {IconType.MEASURE,"/Assets/Icons/icon_length.png"},
            {IconType.CHANGESOIL,"/Assets/Icons/icon_soilchange.png"},
            {IconType.BLOOMING,"/Assets/Icons/icon_blooming.png"},
            {IconType.DECEASED,"/Assets/Icons/icon_deceased.png"},
            {IconType.ILLUMINANCE,"/Assets/Icons/icon_illuminance.png"},
            {IconType.MISTING,"/Assets/Icons/icon_misting.png"},
            {IconType.PH,"/Assets/Icons/icon_ph.png"},
            {IconType.POLLINATION,"/Assets/Icons/icon_pollination.png"},
            {IconType.SPROUTING,"/Assets/Icons/icon_sprouting.png"},
            {IconType.HARVESTING,IconFolder+"icon_harvesting.png"},
            {IconType.PRUNING,IconFolder+"icon_pruning.png"}
        };

    }


    /*
   * Returns Visibility.Visible if value is nonzero
   */
    public class NonZeroToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }

            int i = (int)value;

            if (i == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }





    /*
     * Returns Visibility.Visible if value is zero
     */
    public class ZeroToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }

            int i = (int)value;

            if (i == 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




    /*
     * Returns Visibility.Collapsed if list is empty
     */
    public class EmptyListToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            IEnumerable<Object> list = (IEnumerable<Object>)value;

            if (list.Count() == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }

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

    public class NullToBooleanConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            //return value == null ? Visibility.Collapsed : Visibility.Visible;
            //if (targetType != typeof(Visibility))
            //    throw new InvalidOperationException("Can only convert to Visibility");


            if (parameter == null)
                return value == null ? false : true;
            else
                return value != null ? false : true;

        }

        public object ConvertBack(object v, Type targetType, object parameter,
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

            uint[] d = null;
            if (parameter != null)
            {
                try
                {
                    d = ((string)parameter)
                        .Split('x')
                        .Select(x => uint.Parse(x))
                        .ToArray();

                }
                catch { }

            }

            try
            {
                Photo x = (Photo)value;
                if (d != null && d.Length == 2)
                {
                    x.Width = d[0];
                    x.Height = d[1];
                    x.DimensionsType = DimensionsType.LOGICAL;
                }
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


    }


    public class IconTypeToIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var d = parameter == null ? ConverterHelpers.BigIcons : ConverterHelpers.SmallIcons;

            try
            {
                IconType x = (IconType)value;

                var defaultPhoto = new Photo()
                {
                    LocalUri = d[x],
                    LocalFullPath = d[x]
                };

                return defaultPhoto.ToBitmapImage();

            }
            catch (InvalidCastException)
            {

            }
            catch (ArgumentNullException)
            {

            }
            catch (KeyNotFoundException)
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


    public class IconTypeToIconUriConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var d = parameter == null ? ConverterHelpers.BigIcons : ConverterHelpers.SmallIcons;

            try
            {
                IconType x = (IconType)value;

                return new Uri(d[x], UriKind.RelativeOrAbsolute);

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

    public class CaseConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
                return null;

            var v = value as string;
            if (v == null)
                return null;

            return parameter == null ? v.ToUpperInvariant() : v.ToLowerInvariant();

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