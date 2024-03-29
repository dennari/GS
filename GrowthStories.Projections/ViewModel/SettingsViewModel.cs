﻿using System;
using System.Reactive.Linq;
using Growthstories.Domain.Messaging;
using ReactiveUI;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{



    public class SettingsViewModel : RoutableViewModel, ISettingsViewModel
    {


        public IReactiveCommand NavigateToAbout { get; protected set; }
        public IReactiveCommand SignOutCommand { get; protected set; }
        public IReactiveCommand SignInCommand { get; protected set; }
        public IReactiveCommand SignUpCommand { get; protected set; }
        public IReactiveCommand SynchronizeCommand { get; protected set; }


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


        private bool _GSLocationServicesEnabled;
        public bool GSLocationServicesEnabled
        {
            get
            {
                return _GSLocationServicesEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _GSLocationServicesEnabled, value);
            }
        }


        private bool _PhoneLocationServicesEnabled;
        public bool PhoneLocationServicesEnabled
        {
            get
            {
                return _PhoneLocationServicesEnabled;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _PhoneLocationServicesEnabled, value);
            }
        }


        private async Task DisableGSLocationServices()
        {
            await App.HandleCommand(new SetLocationEnabled(App.User.Id, false));

            // delete location infos for all plants
            foreach (var p in Plants)
            {
                if (p.Location != null)
                {
                    var cmd = new SetLocation(p.Id, null);
                    await App.HandleCommand(cmd);
                }
            }
        }


        public void EnableLocationServices()
        {
            var kludge = new ReactiveCommand();
            kludge.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => _EnableLocationServices());
            kludge.Execute(null);
        }


        private async Task _EnableLocationServices()
        {
            var location = await App.GetLocation();
            if (location != null)
            {
                // mark location acquisition as enabled
                await App.HandleCommand(new SetLocationEnabled(App.User.Id, true));

                // share location info for all current plants
                foreach (var p in Plants)
                {
                    var cmd = new SetLocation(p.Id, location);
                    await App.HandleCommand(cmd);
                }
            }
            else
            {
                // revert toggle switch if unable to get location
                SetGSLocationServicesEnabledNoTrigger(false);
            }
        }

        // I made this public so that we can construct the settingsviewmodel in the AppViewModel and then let the garden
        // set the plans before navigation - Ville
        public IReadOnlyReactiveList<IPlantViewModel> Plants
        {
            get;
            set;
        }


        public SettingsViewModel(IGSAppViewModel app)
            : base(app)
        {

            //this.Plants = plants;

            this.NavigateToAbout = new ReactiveCommand();
            NavigateToAbout.Subscribe(_ => App.Router.Navigate.Execute(new AboutViewModel(App)));


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
                    if (App.Router.NavigationStack.Count > 0)
                    {
                        this.NavigateBack();
                    }
                });
                this.Navigate(svm);
            });


            //var loggingOut = false;
            var logoutObservable = this.LogOutWarning
                .AcceptedObservable
                .SelectMany(async _ =>
                {
                    this.Log().Info("Logging out");
                    //loggingOut = true;
                    App.ShowPopup.Execute(this.LogOutProgress);
                    var r = await App.SignOut();
                    App.ShowPopup.Execute(null);
                    //loggingOut = false;

                    return r;
                }).Publish();

            logoutObservable.Connect();

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


            App.WhenAnyValue(x => x.UserEmail).ToProperty(this, x => x.Email, out _Email);

            //Observable.Merge(
            //    App.WhenAnyValue(x => x.User, x => x != null ? x.Email : null)
            //        .Where(x => loggingOut == false && x != null),
            //    this.ListenTo<InternalRegistered>().Select(x => x.Email)
            //)
            //.ToProperty(this, x => x.Email, out _Email);


            this.SharedByDefault = new ButtonViewModel()
            {
                IsEnabled = false,
            };

            this.SynchronizeCommand.CanExecuteObservable.Subscribe(x => this.CanSynchronize = x);

            this.SynchronizeCommand.Subscribe(x =>
            {
                App.ShowPopup.Execute(App.SyncPopup);

            });

            this.SynchronizeCommand.RegisterAsyncTask(async (_) =>
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

                    return;
                }
                var res = await App.Synchronize();
                App.ShowPopup.Execute(null);
                if (res == null)
                    return;
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
            }).Publish().Connect();

            App.WhenAnyValue(x => x.PhoneLocationServicesEnabled)
                .Subscribe(x => this.PhoneLocationServicesEnabled = x);

            App.WhenAnyValue(x => x.GSLocationServicesEnabled)
                .Subscribe(x =>
                    {
                        SetGSLocationServicesEnabledNoTrigger(x);
                    });

            App.Router.CurrentViewModel
                .Where(vm => vm == this)
                .Subscribe(_ =>
            {
                App.UpdatePhoneLocationServicesEnabled();
            });


            this.WhenAnyValue(x => x.GSLocationServicesEnabled).Skip(1).Subscribe(_ =>
            {
                if (!AllowTriggering)
                {
                    return;
                }
                if (GSLocationServicesEnabled)
                {
                    EnableLocationServices();
                }
                else
                {
                    PossiblyDisableLocationServices();
                }
            });

        }
        public bool AllowTriggering = true;


        //public void LocationServicesEnabledUpdated()
        //{
        //    if (!AllowTriggering)
        //    {
        //        return;
        //    }
        //    AllowTriggering = false;

        //    if (GSLocationServicesEnabled)
        //    {
        //        EnableLocationServices();
        //    }
        //    else
        //    {
        //        PossiblyDisableLocationServices();
        //    }
        //}


        public void SetGSLocationServicesEnabledNoTrigger(bool enabled)
        {
            this.AllowTriggering = false;
            this.GSLocationServicesEnabled = enabled;
            this.AllowTriggering = true;
        }


        public void PossiblyDisableLocationServices()
        {
            SetGSLocationServicesEnabledNoTrigger(false);

            var pvm = new PopupViewModel()
            {
                Caption = "Are you sure?",
                Message = "Changing this settings will remove location data for all your plants.",
                LeftButtonContent = "Yes",
                RightButtonContent = "Cancel",
                IsLeftButtonEnabled = true,
                IsRightButtonEnabled = true,
            };

            pvm.AcceptedObservable.Subscribe(_ =>
            {
                DisableGSLocationServices();
            });

            pvm.DismissedObservable.Subscribe(x =>
            {
                var res = (PopupResult)x;

                if (res != PopupResult.LeftButton)
                {
                    // undo toggle
                    SetGSLocationServicesEnabledNoTrigger(true);
                }
            });

            App.ShowPopup.Execute(pvm);
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
                        Message = "Are you sure you wish to sign out? This will clear all your data from the application.",
                        LeftButtonContent = "yes",
                        RightButtonContent = "no",
                        IsRightButtonEnabled = true
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
                        ProgressMessage = "Please wait while Growth Stories is cleaning up",
                        IsLeftButtonEnabled = false,
                        IsRightButtonEnabled = false,
                        DismissOnBackButton = false
                    };
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
