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
using BindableApplicationBar;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using Ninject;
using Growthstories.Domain;
using Microsoft.Phone.Scheduler;
using GrowthStories.UI.WindowsPhone.BA;


namespace Growthstories.UI.WindowsPhone
{

    public class MainViewBase : GSView<MainViewModel>
    {

    }

    public partial class MainView : MainViewBase
    {


        public MainView()
        {
            InitializeComponent();
            //FriendsSelector.SelectedItem = null;
        }


        private void UpdateTiles(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var app = ViewModel.App as AppViewModel;
            var uip = app.Kernel.Get<IUIPersistence>();

            GSTileUtils.UpdateTilesAndInfos(app, uip);
        }


        private void LaunchBackgroundAgent(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ScheduledAgent.RegisterScheduledTask();
            ScheduledActionService.LaunchForTest(ScheduledAgent.TASK_NAME, TimeSpan.FromSeconds(10));
        }


        private void ConfigureBackgroundAgent(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ScheduledAgent.RegisterScheduledTask();
        }


        /*
        private void FriendsSelector_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ViewModel.FriendsVM.RaisePropertyChanged("SelectedFriend");
        }
         */



        //private void FriendsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count == 0)
        //        return;
        //    var item = e.AddedItems[0];
        //    if (item == null || !(item is IGardenViewModel))
        //        return;

        //    ViewModel.FriendsVM.FriendTapped.Execute(item);

        //    ((LongListSelector)sender).SelectedItem = null;
        //}



    }
}