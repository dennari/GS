using System;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;


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
        private IDisposable GardenVMSubscription = Disposable.Empty;
        protected override void OnViewModelChanged(MainViewModel vm)
        {
            //base.OnViewModelChanged(vm);

            GardenVMSubscription.Dispose();
            GardenVMSubscription = vm.WhenAnyValue(x => x.GardenVM).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                this.GardenView.ViewModel = vm.GardenVM;

            });

        }
        //private void UpdateTiles(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    var app = ViewModel.App as AppViewModel;
        //    var uip = app.Kernel.Get<IUIPersistence>();
        //}


        //private void LaunchBackgroundAgent(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    ScheduledAgent.RegisterScheduledTask();
        //    ScheduledActionService.LaunchForTest(ScheduledAgent.TASK_NAME, TimeSpan.FromSeconds(10));
        //}


        //private void ConfigureBackgroundAgent(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    ScheduledAgent.RegisterScheduledTask();
        //}


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