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
using Telerik;
using Telerik.Windows.Controls;

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
            MyPanorama.SelectionChanged += MyPanorama_SelectionChanged;
        }

        void MyPanorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null || ViewModel.MainVM == null)
                return;

            ViewModel.MainVM.PageChangedCommand.Execute(MyPanorama.SelectedIndex);

        }



        private void DismissPopup(PopupResult result = PopupResult.None)
        {
            if (IsDialogShown)
            {
                IsDialogShown = false;
                if (Popup != null)
                {
                    Popup.Dismiss();
                }

                // we already have a subscription to popup.dismiss, which
                // will take care of calling the dismissedcommand
                // this would create another execution for the dismissedcommand,
                // which can create problems
                //   -- JOJ 3.12.2014
                //if (PopupVm != null && PopupVm.DismissedCommand != null)
                //    PopupVm.DismissedCommand.Execute(result);
                return;
            }
        }

        IDisposable ShowPopupSubscription = Disposable.Empty;
        protected override void OnViewModelChanged(AppViewModel vm)
        {
            //base.OnViewModelChanged(vm);

            ShowPopupSubscription.Dispose();
            ShowPopupSubscription = vm.ShowPopup
              .ObserveOn(RxApp.MainThreadScheduler)
              .Subscribe(x =>
              {
                  var y = x as IPopupViewModel;
                  if (y != null)
                      this.ShowPopup(y);
                  else
                      this.DismissPopup();
              });

        }


        private static System.Windows.Media.Brush PopupForeground
        {
            get
            {
                return (System.Windows.Media.Brush)(Application.Current.Resources["GSTextBoxBrush"]);
            }
        }


        private StackPanel ProgressPopupContent(IPopupViewModel pvm)
        {
            StackPanel sp = new StackPanel()
            {
                Margin = new Thickness(0, 12, 0, 0)
            };

            sp.Children.Add(
                new ProgressBar()
                {
                    IsIndeterminate = true,
                    IsEnabled = true,
                    Margin = new Thickness(0, 12, 0, 12),
                    Foreground = PopupForeground
                }
            );

            sp.Children.Add(
                new TextBlock()
                {
                    Style = (Style)(Application.Current.Resources["GSTextBlockStyle"]),
                    Text = pvm.ProgressMessage,
                    Foreground = PopupForeground,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = System.Windows.TextAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 24,
                    Width = 430,
                    //FontWeight = FontWeights.Light,
                }
            );
            return sp;
        }


        private object PopupContent(IPopupViewModel pvm)
        {
            switch (pvm.Type)
            {
                case PopupType.PROGRESS:
                    return ProgressPopupContent(pvm);
            }
            return null; // default content
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
                IsFullScreen = x.IsFullScreen,
                Foreground = PopupForeground,
                Background = (System.Windows.Media.Brush)(Application.Current.Resources["GSWhiteBrush"]),
                BorderBrush = PopupForeground
            };

            var pc = PopupContent(x);
            if (pc != null)
            {
                popup.Content = pc;
            }

            popup.ApplyTemplate();

            popup.Dismissed += (s1, e1) =>
            {
                if (x.DismissedCommand != null)
                {
                    x.DismissedCommand.Execute(
                        e1.Result == CustomMessageBoxResult.LeftButton ?
                        PopupResult.LeftButton : PopupResult.RightButton);
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

            if (ee != null || this.ViewModel.Router.NavigationStack.Count == 0)
            {
                ViewModel.WhenAnyValue(x => x.MainVM)
                    .Where(x => x != null)
                    .Take(1)
                    .Subscribe(x => ViewModel.Router.Navigate.Execute(x));
            }
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
            base.OnBackKeyPress(e);

            if (IsDialogShown)
            {
                // popups have their own subscription to the backkeypress,
                // so there is no need to call this here
                // we still need to cancel the event though
                //  -- JOJ 3.1.2014
                // DismissPopup(PopupResult.BackButton);

                e.Cancel = true;
                return;
            }

            ViewModel.BackKeyPressedCommand.Execute(e);
            if (e.Cancel == true)
            {
                return;
            }

            if (!ViewModel.Router.NavigateBack.CanExecute(null))
            {
                return;
            }

            e.Cancel = true;

            ViewModel.NavigatingBack = true;
            ViewModel.Router.NavigateBack.Execute(null);
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