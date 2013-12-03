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
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using Growthstories.UI.ViewModel;
using AppViewModel = Growthstories.UI.WindowsPhone.ViewModels.AppViewModel;
using System.Threading.Tasks;
using Growthstories.Sync;


namespace Growthstories.UI.WindowsPhone
{

    public class MainWindowBase : GSPage<AppViewModel>
    {

    }

    public partial class MainWindow : MainWindowBase
    {


        //private Task<IAuthUser> InitializeTask;

        public MainWindow()
        {
            InitializeComponent();
            //this.SetBinding(ViewModelProperty, new Binding());
            ViewModel = new AppViewModel();
            ViewModel.ShowPopup
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    var y = x as IPopupViewModel;
                    if (y != null)
                        this.ShowPopup(y);
                    else
                        this.DismissPopup();
                });

            //this.InitializeTask = Task.Run(async () => await ViewModel.Initialize());
        }

        private void DismissPopup(PopupResult result = PopupResult.None)
        {
            if (IsDialogShown)
            {
                IsDialogShown = false;
                if (Popup != null)
                    Popup.Dismiss();
                if (PopupVm != null && PopupVm.DismissedCommand != null)
                    PopupVm.DismissedCommand.Execute(result);

                return;
            }
        }


        CustomMessageBox Popup;
        IPopupViewModel PopupVm;
        private void ShowPopup(IPopupViewModel x)
        {
            var popup = new CustomMessageBox()
            {
                Caption = x.Caption,
                Message = x.Message,
                LeftButtonContent = x.LeftButtonContent,
                IsRightButtonEnabled = x.IsRightButtonEnabled,
                IsLeftButtonEnabled = x.IsLeftButtonEnabled,
                RightButtonContent = x.RightButtonContent,
                IsFullScreen = x.IsFullScreen
            };

            popup.Dismissed += (s1, e1) =>
            {
                if (x.DismissedCommand != null)
                {
                    x.DismissedCommand.Execute(e1.Result == CustomMessageBoxResult.LeftButton ? PopupResult.LeftButton : PopupResult.RightButton);
                }
                this.IsDialogShown = false;
            };

            this.IsDialogShown = true;
            this.PopupVm = x;
            this.Popup = popup;
            popup.Show();
        }



        //public static readonly DependencyProperty ViewModelProperty =
        //   DependencyProperty.Register("ViewModel", typeof(Growthstories.UI.WindowsPhone.ViewModels.AppViewModel), typeof(MainWindow), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));

        //public AppViewModel ViewModel
        //{
        //    get
        //    {
        //        return (AppViewModel)GetValue(ViewModelProperty);
        //    }
        //    set
        //    {
        //        if (value != null && value != ViewModel)
        //            SetValue(ViewModelProperty, value);
        //    }
        //}

        //object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (AppViewModel)value; } }


        /// <summary>
        /// We get here on the initial load AND whenever we resume, i.e. from tasks
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            IDictionary<string, string> qs = this.NavigationContext.QueryString;
            Exception ee = null;
            if (qs.Count > 0)
                try
                {
                    this.NavigateWithDeepLink(qs);
                    return;
                }
                catch (Exception E)
                {
                    ee = E;
                }

            if (ee != null || this.ViewModel.Router.NavigationStack.Count == 0) // don't do anything if this isn't the initial load
                this.ViewModel.Router.Navigate.Execute(new MainViewModel(this.ViewModel));

        }

        private IDisposable PlantNavigationSubscription = Disposable.Empty;
        protected void NavigateWithDeepLink(IDictionary<string, string> qs)
        {

            var id = Guid.Parse(qs["id"]);
            IAuthUser user = ViewModel.User;
            if (user == null)
            {
                ViewModel.Initialize().Wait();
                user = ViewModel.User;
            }
            var garden = new GardenViewModel(user, ViewModel);
            var pivot = new GardenPivotViewModel(garden);

            //var x = garden.Plants.BeforeItemsAdded.Where(y => y.Id == id).Take(1).ObserveOn(RxApp.MainThreadScheduler).Wait();
            //pivot.SelectedItem = x;
            this.ViewModel.Router.Navigate.Execute(pivot);

            PlantNavigationSubscription.Dispose();
            PlantNavigationSubscription = garden.Plants.ItemsAdded.Where(x => x.Id == id).Take(1).ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                if (x.Id == id)
                {
                    pivot.SelectedItem = x;

                }
            });


            //var user = ViewModel.Initialize().Wait()
            //var garden = new GardenPivotViewModel(this.ViewModel.Resolver.GetService<IGardenViewModel>());
            //var plantTask = Task.Run(async () => await garden.Plants.ItemsAdded
            //        .Where(x => x.Id == id)
            //        .Take(1));
            //var plant = plantTask.Wait();
            //garden.SelectedItem = plant;
            //Task.Run(async () =>
            //{

            //    var user = await 
            //    var garden = new GardenPivotViewModel(this.ViewModel.Resolver.GetService<IGardenViewModel>());

            //    ViewModel.CurrentPlants(user).Subscribe(

            //     );

            //    //.Subscribe();


            //    this.ViewModel.Router.Navigate.Execute(garden);

            //}).Wait();

        }

        private bool IsDialogShown;

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            //if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit?",
            //                        MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            //{
            base.OnBackKeyPress(e);
            if (IsDialogShown)
            {
                DismissPopup(PopupResult.BackButton);
                return;
            }

            ViewModel.BackKeyPressedCommand.Execute(e);
            if (e.Cancel == true)
            {
                return;
            }
            if (!ViewModel.Router.NavigateBack.CanExecute(null)) return;

            e.Cancel = true;
            ViewModel.Router.NavigateBack.Execute(null);
            //e.Cancel = true;

            //}
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
            //var cvm = ViewModel.Router.GetCurrentViewModel() as Growthstories.UI.ViewModel.IControlsPageOrientation;
            //if (cvm != null)
            //{
            ViewModel.PageOrientationChangedCommand.Execute((Growthstories.UI.ViewModel.PageOrientation)e.Orientation);
            //}

        }
    }
}