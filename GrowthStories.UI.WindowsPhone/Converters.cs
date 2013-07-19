
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

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


}