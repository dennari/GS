using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using System.Reactive.Linq;
using Growthstories.Domain.Entities;
using Growthstories.Core;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Growthstories.UI.ViewModel
{




    public class SettingsViewModel : RoutableViewModel, ISettingsViewModel
    {


        bool InsideJob = false;
        public IReactiveCommand NavigateToAbout { get; protected set; }
        public IReactiveCommand WarnCommand { get; protected set; }
        public IReactiveCommand WarningDismissedCommand { get; protected set; }
        public IReactiveCommand SignOutCommand { get; protected set; }
        public IReactiveCommand SynchronizeCommand { get; protected set; }
        public IReactiveCommand MaybeSignOutCommand { get; protected set; }

        private string _Email;
        public string Email
        {
            get
            {
                return _Email;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _Email, value);
            }
        }

        private bool _CanSynchronize = true;
        public bool CanSynchronize
        {
            get
            {
                return _CanSynchronize;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _CanSynchronize, value);
            }
        }


        private bool _RefreshSignInSwitch;
        private bool RefreshSignInSwitch
        {
            get
            {
                return _RefreshSignInSwitch;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _RefreshSignInSwitch, value);
            }
        }

        public SettingsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this.NavigateToAbout = app.Router.NavigateCommandFor<IAboutViewModel>();

            this.LocationServices = new ButtonViewModel()
            {
                IsEnabled = false,

            };
            this.SignIn = new ButtonViewModel();
            this.WarnCommand = new ReactiveCommand();
            this.WarningDismissedCommand = new ReactiveCommand();
            this.SignOutCommand = new ReactiveCommand();
            this.MaybeSignOutCommand = new ReactiveCommand();
            this.SynchronizeCommand = new ReactiveCommand();

            Observable.CombineLatest(
                App.WhenAnyValue(x => x.User).Where(x => x != null),
                App.WhenAnyValue(x => x.IsRegistered),
                this.WhenAnyValue(x => x.RefreshSignInSwitch),
                (x, y, z) => Tuple.Create(x, y, z)
             )
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                InsideJob = true;
                if (x.Item2 && x.Item1.AccessToken != null)
                {
                    this.SignIn.IsEnabled = true;
                    this.Email = x.Item1.Email;
                }
                else
                    this.SignIn.IsEnabled = false;

                InsideJob = false;
            });

            this.SignIn.WhenAnyValue(x => x.IsEnabled).Skip(1).Subscribe(x =>
            {
                if (InsideJob)
                {

                    return;
                }
                this.RefreshSignInSwitch = !this.RefreshSignInSwitch;

                if (x == false && App.IsRegistered)
                {

                    App.ShowPopup.Execute(this.LogOutWarning);


                }
                if (x == true && !App.IsRegistered)
                    this.Navigate(new SignInRegisterViewModel(App));
            });



            this.MaybeSignOutCommand.OfType<PopupResult>().Where(x => x == PopupResult.LeftButton).Subscribe(_ => this.SignOutCommand.Execute(null));

            var logOutResult = this.SignOutCommand.RegisterAsyncTask(async (_) => await App.SignOut());
            logOutResult.Subscribe(x => this.Navigate(new MainViewModel(App)));

            this.SharedByDefault = new ButtonViewModel()
            {
                IsEnabled = false,

            };





            this.SynchronizeCommand.Subscribe(x =>
            {
                this.CanSynchronize = false;
                App.SynchronizeCommand.Execute(null);
            });

            App.SyncResults.Subscribe(x =>
            {
                this.CanSynchronize = true;

            });








        }

        private IPopupViewModel _LogOutWarning;
        private IPopupViewModel LogOutWarning
        {
            get
            {
                if (_LogOutWarning == null)
                {
                    _LogOutWarning = new PopupViewModel()
                    {
                        Caption = "Confirmation",
                        Message = "Are you sure you wish to sign out?",
                        LeftButtonContent = "yes",
                        DismissedCommand = this.MaybeSignOutCommand
                    };
                }
                return _LogOutWarning;
            }
        }





        private IButtonViewModel _LocationServices;
        public IButtonViewModel LocationServices
        {
            get
            {
                return _LocationServices;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LocationServices, value);
            }
        }

        private IButtonViewModel _SharedByDefault;
        public IButtonViewModel SharedByDefault
        {
            get
            {
                return _SharedByDefault;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SharedByDefault, value);
            }
        }

        private IButtonViewModel _SignIn;
        public IButtonViewModel SignIn
        {
            get
            {
                return _SignIn;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SignIn, value);
            }
        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return false; }
        }

        protected bool _ProgressIndicatorIsVisible;
        public bool ProgressIndicatorIsVisible
        {
            get { return _ProgressIndicatorIsVisible; }
            set { this.RaiseAndSetIfChanged(ref _ProgressIndicatorIsVisible, value); }
        }

        public bool SystemTrayIsVisible
        {
            get { return false; }
        }
    }



    public class AboutViewModel : RoutableViewModel, IAboutViewModel
    {




        public AboutViewModel(IGSAppViewModel app)
            : base(app)
        {



        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return false; }
        }

        protected bool _ProgressIndicatorIsVisible;
        public bool ProgressIndicatorIsVisible
        {
            get { return _ProgressIndicatorIsVisible; }
            set { this.RaiseAndSetIfChanged(ref _ProgressIndicatorIsVisible, value); }
        }

        public bool SystemTrayIsVisible
        {
            get { return false; }
        }
    }

}
