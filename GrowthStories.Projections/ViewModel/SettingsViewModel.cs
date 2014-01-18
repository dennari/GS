using System;
using System.Reactive.Linq;
using ReactiveUI;
using Growthstories.Domain.Messaging;

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

        private ObservableAsPropertyHelper<bool> _IsRegistered;
        public bool IsRegistered
        {
            get
            {
                return _IsRegistered.Value;
            }
        }


        private ObservableAsPropertyHelper<string> _Email;
        public string Email
        {
            get
            {
                return _Email.Value;
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

            var isRegisteredObservable = App.WhenAnyValue(x => x.IsRegistered);


            //this.WarnCommand = new ReactiveCommand();
            //this.WarningDismissedCommand = new ReactiveCommand();

            this.SignInCommand = new ReactiveCommand();
            //this.SignInCommand = new ReactiveCommand(hasUserAndIsRegistered.Select(x => !x.Item2));
            this.SignInCommand.Subscribe(x =>
            {
                SignInRegisterViewModel svm = new SignInRegisterViewModel(App);
                svm.SignInMode = true;
                // after successfull signin we get back to mainviewmodel automatically
                this.Navigate(svm);
            }
            );

            this.SignUpCommand = new ReactiveCommand();
            //this.SignUpCommand = new ReactiveCommand(hasUserAndIsRegistered.Select(x => !x.Item2));
            this.SignUpCommand.Subscribe(x =>
            {
                SignInRegisterViewModel svm = new SignInRegisterViewModel(App);
                svm.SignInMode = false;
                svm.Response.Where(y => y.Item2 == RegisterResponse.success).Take(1).Subscribe(y =>
                {
                    this.NavigateBack();
                });
                this.Navigate(svm);
            });


            var loggingOut = false;
            var logoutObservable = this.LogOutWarning
                .AcceptedObservable
                .SelectMany(async _ =>
                {
                    this.Log().Info("Logging out");
                    loggingOut = true;
                    App.ShowPopup.Execute(this.LogOutProgress);
                    var r = await App.SignOut();
                    App.ShowPopup.Execute(null);
                    loggingOut = false;

                    return r;


                }).Publish();



            logoutObservable.Connect();
            //.ObserveOn(RxApp.MainThreadScheduler)
            //.Subscribe(x => this.Navigate(new MainViewModel(App)));


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
                Command = Observable.Return(true).ToCommandWithSubscription(_ => App.ShowPopup.Execute(this.LogOutWarning))
            };
            this.SignUpButton = new ButtonViewModel()
            {
                Text = "register",
                IconType = IconType.SIGNUP,
                Command = SignUpCommand
            };

            
            isRegisteredObservable
                .Do(x =>
                {

                    this._AppBarButtons.Remove(this.SignUpButton);
                    this._AppBarButtons.Remove(this.SignInButton);
                    this._AppBarButtons.Remove(this.SignOutButton);

                    if (x)
                    {
                        this._AppBarButtons.Add(this.SignOutButton);
                    }
                    else
                    {
                        this._AppBarButtons.Add(this.SignInButton);
                        this._AppBarButtons.Add(this.SignUpButton);
                    }

                })
                .ToProperty(this, x => x.IsRegistered, out _IsRegistered);

            App.WhenAnyValue(x => x.User.Email).Where(_ => loggingOut == false).ToProperty(this, x => x.Email, out _Email);

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
                        Caption = "Data connection required",
                        Message = "Synchronizing requires a data connection. Please enable one in your phone's settings and try again.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK"
                    };
                    App.ShowPopup.Execute(pvm);

                }
                else
                {
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
                    };
                    //_LogOutWarning.DismissedObservable.Subscribe(_ => )
                }
                return _LogOutWarning;
            }
        }

        private IPopupViewModel _LogOutProgress;
        private IPopupViewModel LogOutProgress
        {
            get
            {
                if (_LogOutProgress == null)
                {
                    _LogOutProgress = new ProgressPopupViewModel()
                    {
                        Caption = "Logging out",
                        Message = "Please hold on",
                        IsLeftButtonEnabled = false,
                        IsRightButtonEnabled = false
                    };
                    //_LogOutProgress.DismissedObservable.Subscribe(_ => )
                }
                return _LogOutProgress;
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
