using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Growthstories.UI.WindowsPhone
{


    class SIPHelper
    {

        public static int GetSipHeight()
        {
            return 336;
        }


        public static void SIPGotVisible(ContentControl c)
        {
            c.Height = GetSipHeight();
            c.Visibility = Visibility.Visible;
        }


        public static void SIPGotHidden(ContentControl c)
        {
            c.Visibility = Visibility.Collapsed;
        }


    }
}
