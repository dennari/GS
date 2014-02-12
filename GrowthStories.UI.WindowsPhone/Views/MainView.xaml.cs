using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Scheduler;
using ReactiveUI;

namespace Growthstories.UI.WindowsPhone
{

    public class MainViewBase : GSView<IMainViewModel>
    {
        public MainViewBase()
        {
            this.SetBinding(ViewModelProperty, new Binding());

        }

        protected override void OnViewModelChanged(IMainViewModel vm)
        {
            base.OnViewModelChanged(vm);
        }
    }


    public partial class MainView : MainViewBase
    {


        public MainView()
        {
            InitializeComponent();
            //FriendsSelector.SelectedItem = null;
        }

        private IDisposable GardenVMSubscription = Disposable.Empty;
        protected override void OnViewModelChanged(IMainViewModel vm)
        {
            //base.OnViewModelChanged(vm);

            GardenVMSubscription.Dispose();
            GardenVMSubscription = vm.WhenAnyValue(x => x.GardenVM).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                this.GardenView.ViewModel = vm.GardenVM;

            });
        }

        private void CauseException(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e = null;
            var a = e.Handled;
        }


        private void ClearMockIAP(object sender, System.Windows.Input.GestureEventArgs e)
        {
            #if DEBUG
            MockIAPLib.MockIAP.ClearCache();
            #endif
        }

        private void LaunchBackgroundAgent(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BAUtils.RegisterScheduledTask();
            ScheduledActionService.LaunchForTest(BAUtils.TASK_NAME, TimeSpan.FromSeconds(10));
        }


        private void ConfigureBackgroundAgent(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BAUtils.RegisterScheduledTask();
        }

        private void MainViewBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.Log().Info("MainView Loaded in {0}", GSAutoSuspendApplication.LifeTimer.ElapsedMilliseconds);
        }


    }
}