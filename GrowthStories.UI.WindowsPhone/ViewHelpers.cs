using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace Growthstories.UI.WindowsPhone
{



    public static class ViewHelpers
    {



        private static Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Color GetColorFromHex(string hexColorString)
        {
            if (hexColorString == null)
                throw new NullReferenceException("Hex string can't be null.");

            // Regex match the string
            var match = _hexColorMatchRegex.Match(hexColorString);

            if (!match.Success)
                throw new InvalidCastException(string.Format("Can't convert string \"{0}\" to argb or rgb color. Needs to be 6 (rgb) or 8 (argb) hex characters long. It can optionally start with a #.", hexColorString));

            // a value is optional
            byte a = 255, r = 0, b = 0, g = 0;
            if (match.Groups["a"].Success)
                a = System.Convert.ToByte(match.Groups["a"].Value, 16);
            // r,b,g values are not optional
            r = System.Convert.ToByte(match.Groups["r"].Value, 16);
            b = System.Convert.ToByte(match.Groups["b"].Value, 16);
            g = System.Convert.ToByte(match.Groups["g"].Value, 16);
            return Color.FromArgb(a, r, b, g);
        }

        public static Color ToColor(this string This)
        {
            return GetColorFromHex(This);
        }



        public static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (FrameworkElement)sender;
                var vm = e.NewValue;

                if (view.DataContext != vm)
                    view.DataContext = vm;

                var viewF = view as IReportViewModelChange;
                if (viewF != null)
                    viewF.ViewModelChangeReport(vm);


            }
            catch { }
        }
    }
}
