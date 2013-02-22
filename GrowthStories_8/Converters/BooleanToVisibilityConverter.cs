// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanToVisibilityConverter.cs" company="saramgsilva">
//   Copyright (c) 2012 saramgsilva. All rights reserved.
// </copyright>
// <summary>
//   Value converter that translates true to <see cref="Visibility.Visible" /> and false to
//   <see cref="Visibility.Collapsed" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Growthstories.WP8.Converters
{
    using System;
#if SILVERLIGHT
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
#else
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
#endif
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is negation.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is negation; otherwise, <c>false</c>.
        /// </value>
        public bool IsNegation { get; set; }

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>The value converted</returns>
#if SILVERLIGHT
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
#else
       public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            if (IsNegation)
            {
                return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
            }

            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
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
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
