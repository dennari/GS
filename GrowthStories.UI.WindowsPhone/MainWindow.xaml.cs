using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Controls;
using ReactiveUI;
using ReactiveUI.Mobile;

namespace Growthstories.UI.WindowsPhone
{

    public class MainWindowBase : GSPage<IGSAppViewModel>, IEnableLogger
    {
        private readonly SuspensionHost LifeTimeHelper;

        public MainWindowBase()
        {


            LifeTimeHelper = GSAutoSuspendApplication.SuspensionHost;

            if (LifeTimeHelper != null)
            {
                LifeTimeHelper.IsResuming.Subscribe(HandleResuming);
                LifeTimeHelper.IsUnpausing.Subscribe(HandleUnpausing);
            }

        }

        protected virtual void HandleUnpausing(Unit _)
        {
            this.Log().Info("Application Unpausing");
            HandleResuming(_);

        }

        protected virtual void HandleResuming(Unit _)
        {
            this.Log().Info("Application Resuming");
            if (ViewModel != null)
                ViewModel.HandleApplicationActivated();


        }

        protected override void OnViewModelChanged(IGSAppViewModel vm)
        {
            base.OnViewModelChanged(vm);

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

            SetDismissPopupAllowedSubscription.Dispose();
            SetDismissPopupAllowedSubscription = vm.SetDismissPopupAllowedCommand
                // this was important not to do in RxApp.MainThreadScheduler, 
                // as it could delay setting this flag and mess things up 
                //.ObserveOn(RxApp.MainThreadScheduler) 
                .OfType<bool>()
                .Subscribe(x =>
                {
                    SetDismissPopupAllowed(x);
                });
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



        #region POPUP

        private void DismissPopup(PopupResult result = PopupResult.None)
        {

            if (IsDialogShown)
            {
                IsDialogShown = false;
                if (Popup != null)
                {
                    Popup.Dismiss();
                }

            }
        }

        IDisposable ShowPopupSubscription = Disposable.Empty;
        IDisposable SetDismissPopupAllowedSubscription = Disposable.Empty;

        protected IRoutableViewModel DefaultContentViewModel;
        protected virtual void SetDefaultContentViewModel(IRoutableViewModel vm)
        {
            DefaultContentViewModel = vm;

        }


        public void SetDismissPopupAllowed(bool allowed)
        {
            this.Log().Info("settings dismiss popup allowed in mainwindow to {0}", allowed);
            Popup.DismissOnBackButton = allowed;
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


        GSCustomMessageBox Popup;
        IPopupViewModel PopupVm;
        private void ShowPopup(IPopupViewModel x)
        {

            var popup = new GSCustomMessageBox()
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
                BorderBrush = PopupForeground,
                DismissOnBackButton = x.DismissOnBackButton,
            };

            var pc = PopupContent(x);
            if (pc != null)
            {
                popup.Content = pc;
            }

            popup.ApplyTemplate();
            popup.Dismissed += (s1, e1) =>
            {
                PopupResult res;
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        res = PopupResult.LeftButton;
                        break;

                    case CustomMessageBoxResult.RightButton:
                        res = PopupResult.RightButton;
                        break;

                    default:
                        res = PopupResult.None;
                        break;
                }
                this.Log().Info("Popup dismissed: {0}", res);

                x.Dismiss(res);
                this.IsDialogShown = false;
            };

            this.IsDialogShown = true;
            this.PopupVm = x;
            this.Popup = popup;
            this.Log().Info("Popup \"{0}\" shown", popup.Caption ?? string.Empty);

            popup.Show();
        }



        /// <summary>
        /// We get here on the initial load AND whenever we resume, i.e. from tasks
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Log().Info("OnNavigatedTo");


        }




        private bool IsDialogShown;


        protected override void OnBackKeyPress(CancelEventArgs e)
        {

            base.OnBackKeyPress(e);

            if (Popup != null)
            {
                this.Log().Info("backbutton pressed, dismissOnBackButton is " + Popup.DismissOnBackButton);
            }

            if (IsDialogShown && Popup != null && Popup.DismissOnBackButton)
            {
                // popups have their own subscription to the backkeypress,
                // so there is no need to call this here
                // we still need to cancel the event though for reactiveUI
                //
                //  -- JOJ 3.1.2014
                // DismissPopup(PopupResult.BackButton);
                e.Cancel = true;
                this.Log().Info("dialog is shown, canceling ");

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

            if (!IsDialogShown)
            {
                this.Log().Info("navigating back");
                ViewModel.NavigatingBack = true;
                ViewModel.Router.NavigateBack.Execute(null);
            }
            else
            {
                this.Log().Info("preventing back navigation");
            }
        }




        #endregion

    }



    public partial class MainWindow : MainWindowBase
    {

        //private Task<IAuthUser> InitializeTask;

        public MainWindow()
        {
            this.Log().Info("MainWindow constructor");
            InitializeComponent();

        }



        private bool UILoaded = false;

        protected override void OnViewModelChanged(IGSAppViewModel vm)
        {
            if (MainViewModel != null)
                return;

            MainViewModel = vm.CreateMainViewModel();

            base.OnViewModelChanged(vm);
            if (UILoaded)
            {

                UIAndVMLoaded();

            }


        }

        IMainViewModel MainViewModel;

        private void UIAndVMLoaded()
        {

            ViewModel.Log().Info("MainWindow Loaded in {0}", GSAutoSuspendApplication.LifeTimer.ElapsedMilliseconds);
            ViewModel.MainWindowLoadedCommand.Execute(MainViewModel);
            this.ApplicationBar.IsVisible = true;
            //this.MainView.ViewModel = MainViewModel;

            //this.DataContext = ViewModel;
            //this.DataContext = ViewModel;
        }


        protected void MainWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (UILoaded)
                return;

            UILoaded = true;
            if (MainViewModel != null)
            {
                UIAndVMLoaded();
            }


        }



    }
}