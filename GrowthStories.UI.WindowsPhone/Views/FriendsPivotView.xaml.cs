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
using System.Reactive.Linq;
using EventStore.Logging;

namespace Growthstories.UI.WindowsPhone
{

    public class FriendsPivotViewBase : GSView<IFriendsViewModel>
    {

    }


    public partial class FriendsPivotView : FriendsPivotViewBase
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(FriendsPivotView));

        Guid id;
        static ReactiveCommand Constructed = new ReactiveCommand();


        public FriendsPivotView()
        {
            id = Guid.NewGuid();
            InitializeComponent();

            Logger.Info("initializing new friendspivotview {0}", id);
       
            Constructed.Execute(null);
            Constructed.Take(1).Subscribe(_ => CleanUp());
        }


        private void CleanUp()
        {
            this.ViewModel.Log().Info("cleaning up friendspivot {0}", id);
            
            Friends.ItemsSource = null;
            //Friends.SelectedItem = null;
            ViewHelpers.ClearPivotDependencyValues(Friends);
            
            //LayoutRoot.Children.Clear();
        }


        ~FriendsPivotView()
        {
            NotifyDestroyed(id.ToString());
        }

    }
}