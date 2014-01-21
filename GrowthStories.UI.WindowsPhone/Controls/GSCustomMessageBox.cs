// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;


namespace Growthstories.UI.WindowsPhone
{


    class GSCustomMessageBox : CustomMessageBox
    {


        public GSCustomMessageBox()
            : base()
        {
            DismissOnBackButton = true;
        }


        public bool DismissOnBackButton {get; set; }


        protected override void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DismissOnBackButton)
            {
                base.OnBackKeyPress(sender, e);
            }
            
            // we don't wish to cancel the event, 
            // so back navigation is triggered instead
            // of dismissing the back button
        }


    }


}