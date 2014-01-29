using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using Microsoft.Phone.Controls;
using ReactiveUI;
using AppViewModel = Growthstories.UI.WindowsPhone.ViewModels.ClientAppViewModel;
using ClientPlantViewModel = Growthstories.UI.WindowsPhone.ViewModels.ClientPlantViewModel;

namespace Growthstories.UI.WindowsPhone
{

    public class MainWindowBase : GSPage<AppViewModel>
    {

    }



    public partial class MainWindow : MainWindowBase, IEnableLogger
    {

        //private Task<IAuthUser> InitializeTask;

        public MainWindow()
        {
            this.Log().Info("MainWindow constructor");
            InitializeComponent();

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

            }
        }

        IDisposable ShowPopupSubscription = Disposable.Empty;
        IDisposable SetDismissPopupAllowedSubscription = Disposable.Empty;


        protected override void OnViewModelChanged(AppViewModel vm)
        {
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
            SetDismissPopupAllowedSubscription = vm.SetDismissPopupAllowed
                .ObserveOn(RxApp.MainThreadScheduler)
                .OfType<bool>()
                .Subscribe(x =>
                {
                    SetDismissPopupAllowed(x);
                });
        }


        public void SetDismissPopupAllowed(bool allowed)
        {
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





        private bool firstOnNavigatedTo = true;

        /// <summary>
        /// We get here on the initial load AND whenever we resume, i.e. from tasks
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.Log().Info("OnNavigatedTo");

            if (this.ViewModel.Router.NavigationStack.Count > 0) // we are returning from a chooser or activating via switcher
            {
                // this seems to the only working place were we can trigger
                // something whenever an app is brought to foreground, since
                // the usual events somehow don't work

                this.Log().Info("HandleApplicationActivated");
                ViewModel.HandleApplicationActivated();
                
                // when navigated from secondary tile we also need to update infos on tiles
                if (OnlyPlant != null && OnlyPlant.TileHelper != null)
                {
                    this.Log().Info("updating whether only plant has tile");
                    OnlyPlant.TileHelper.UpdateHasTile();
                }

                return;
            }


            IDictionary<string, string> qs = this.NavigationContext.QueryString;

            Guid plantId = default(Guid);

            if (qs.ContainsKey("id") && Guid.TryParse(qs["id"], out plantId))
            {
                this.NavigateWithDeepLink(plantId);
                return;
            }

            ViewModel.Router.Navigate.Execute(ViewModel.CreateMainViewModel());
        }


        private ClientPlantViewModel OnlyPlant;

        private IDisposable PlantNavigationSubscription = Disposable.Empty;
        protected void NavigateWithDeepLink(Guid plantId)
        {


            // this should actually return immediately
            this.Log().Info("Navigated from tile");
            IPlantViewModel pvm = null;
            try
            {
                //this.Log().Info("Loading plant"); 
                this.Log().Info("Loading plant started");

                var t = new Stopwatch();
                t.Start();
                //pvm = ViewModel.CurrentPlants(plantId: plantId).Where(x => x != null).Take(1)
                //    .Do(x => this.Log().Info("CurrentPlants returned"))
                //    .FirstOrDefaultAsync().Wait();
                pvm = ViewModel.GetSinglePlant(plantId);
                OnlyPlant = pvm as ClientPlantViewModel;
                var a = pvm.Actions; // just to start loading
                t.Stop();
                this.Log().Info("Loading plant took: {0}ms", t.ElapsedMilliseconds);

                ViewModel.Bus.Listen<IEvent>().OfType<AggregateDeleted>().Where(x => x.AggregateId == plantId).Take(1).Subscribe(x =>
                {
                    // get back from plantview, into "nothing"
                    this.ViewModel.Router.NavigateBack.Execute(null);
                    // exit app
                    this.ViewModel.Router.NavigateBack.Execute(null);

                });

            }
            catch (Exception e)
            {

            }

            this.ViewModel.Router.Navigate.Execute(new PlantSingularViewModel(pvm, ViewModel));
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
            ViewModel.NavigatingBack = true;

            this.Log().Info("navigating back");
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