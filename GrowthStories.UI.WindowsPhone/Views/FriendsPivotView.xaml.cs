using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using Growthstories.UI.ViewModel;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{
    public class FriendsPivotViewBase : GSView<IFriendsViewModel>
    {

    }

    public partial class FriendsPivotView : FriendsPivotViewBase
    {


        public FriendsPivotView()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.App.PossiblyAutoSync();
        }


    }
}