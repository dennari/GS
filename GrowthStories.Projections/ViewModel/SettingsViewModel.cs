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


        //bool InsideJob = false;
        public IReactiveCommand NavigateToAbout { get; protected set; }
        //public IReactiveCommand WarnCommand { get; protected set; }
        //public IReactiveCommand WarningDismissedCommand { get; protected set; }
        public IReactiveCommand SignOutCommand { get; protected set; }
        public IReactiveCommand SignInCommand { get; protected set; }
        public IReactiveCommand SignUpCommand { get; protected set; }

        public IReactiveCommand SynchronizeCommand { get; protected set; }
        //public IReactiveCommand MaybeSignOutCommand { get; protected set; }


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


        public SettingsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this.NavigateToAbout = app.Router.NavigateCommandFor<IAboutViewModel>();

            this.LocationServices = new ButtonViewModel()
            {
                IsEnabled = false,

            };
            var hasUserAndIsRegistered = Observable.CombineLatest(
                App.WhenAnyValue(x => x.User).Where(x => x != null),
                App.WhenAnyValue(x => x.IsRegistered),
                (x, y) => Tuple.Create(x, y)
             );
            var canSignOut = hasUserAndIsRegistered.Select(x => x.Item2 && x.Item1.AccessToken != null);

            //this.WarnCommand = new ReactiveCommand();
            //this.WarningDismissedCommand = new ReactiveCommand();

            this.SignInCommand = new ReactiveCommand(hasUserAndIsRegistered.Select(x => !x.Item2));
            this.SignInCommand.Subscribe(x => 
            { 
                SignInRegisterViewModel svm = new SignInRegisterViewModel(App);
                svm.SignInMode = true;
                this.Navigate(svm);
            }
            );

            this.SignUpCommand = new ReactiveCommand(hasUserAndIsRegistered.Select(x => !x.Item2));
            this.SignUpCommand.Subscribe(x => this.Navigate(new SignInRegisterViewModel(App)));

            this.SignOutCommand = new ReactiveCommand(canSignOut);
            this.SignOutCommand.Where(x => x == null).Subscribe(_ => App.ShowPopup.Execute(this.LogOutWarning));
            var logOutResult = this.SignOutCommand.RegisterAsyncTask(async (x) =>
            {
                try
                {
                    if (x != null)
                    {
                        PopupResult r = (PopupResult)x;
                        if (r == PopupResult.LeftButton)
                        {
                            return await App.SignOut();
                        }
                    }
                }

                catch {} // what exceptions are we expecting here?
                
                return null;
            });

            logOutResult.Where(x => x != null).Subscribe(_ => this.Navigate(new MainViewModel(App)));

            this.SynchronizeCommand = new ReactiveCommand();

            this.SignInButton = new ButtonViewModel()
            {
                Text = "sign in",
                IconType = IconType.SIGNIN,
                Command = SignInCommand
            };
            this.SignOutButton = new ButtonViewModel()
            {
                Text = "sign out",
                IconType = IconType.SIGNOUT,
                Command = SignOutCommand
            };
            this.SignUpButton = new ButtonViewModel()
            {
                Text = "sign up",
                IconType = IconType.SIGNUP,
                Command = SignUpCommand
            };
            
            
            hasUserAndIsRegistered
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {

                this._AppBarButtons.Remove(this.SignUpButton);
                this._AppBarButtons.Remove(this.SignInButton);
                this._AppBarButtons.Remove(this.SignOutButton);

                if (x.Item2 && x.Item1.AccessToken != null) {
                    this.Email = x.Item1.Email;
                    this._AppBarButtons.Add(this.SignOutButton);
                
                } else {
                    this.Email = null;
                    this._AppBarButtons.Add(this.SignInButton);
                    this._AppBarButtons.Add(this.SignUpButton);
                }

            });

            this.SharedByDefault = new ButtonViewModel()
            {
                IsEnabled = false,
            };

            this.SynchronizeCommand.Subscribe(x =>
            {
                if (!App.HasDataConnection)
                {
                    PopupViewModel pvm = new PopupViewModel()
                    {
                        Caption = "Data connection reqyured",
                        Message = "Synchronizing requires a data connection. Please enable one in your phone's settings and try again.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK"
                    };
                    App.ShowPopup.Execute(pvm);
                
                } else {
                    this.CanSynchronize = false;
                    App.SynchronizeCommand.Execute(null);

                }
            });

            App.SyncResults.Subscribe((x) =>
            {
                //_SyncResult = x.Item1;
                this.CanSynchronize = true;
            });


            App.UISyncFinished.Subscribe(x =>
            {
                var res = x as Tuple<Sync.AllSyncResult, Sync.GSStatusCode?>;

                if (res.Item1 == Sync.AllSyncResult.Error)
                {
                    PopupViewModel pvm = new PopupViewModel()
                    {
                        Caption = "Failed to synchronize",
                        Message = "Could not synchronize with the Growth Stories servers. Please try again later.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK"
                    };
                    App.ShowPopup.Execute(pvm);
                }
            });

        }

        private Sync.AllSyncResult _SyncResult;


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
                        DismissedCommand = this.SignOutCommand
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

        public IButtonViewModel SignInButton { get; private set; }
        public IButtonViewModel SignOutButton { get; private set; }
        public IButtonViewModel SignUpButton { get; private set; }


        public override string UrlPathSegment
        {
            get { return "settings"; }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
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

        ReactiveList<IButtonViewModel> _AppBarButtons = new ReactiveList<IButtonViewModel>();
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons; }
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
