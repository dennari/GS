// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanNegationConverter.cs" company="saramgsilva">
//   Copyright (c) 2012 saramgsilva. All rights reserved.
// </copyright>
// <summary>
//   Value converter that translates true to false and vice versa.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Growthstories.WP8.Converters
{
    using System;

#if SILVERLIGHT
    using System.Globalization;
    using System.Windows.Data;
#else
    using Windows.UI.Xaml.Data;
#endif
    
    /// <summary>
    /// Value converter that translates true to false and vice versa.
    /// </summary>
    public sealed class BooleanNegationConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The value converted.</returns>
#if SILVERLIGHT
       public object Convert(object value, Type targetType, object parameter, CultureInfo language)
#else
       public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            return !(value is bool && (bool)value);
        }

        /// <summary>
        /// Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The value that was converted back</returns>
#if SILVERLIGHT
       public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            return !(value is bool && (bool)value);
        }
    }
}
