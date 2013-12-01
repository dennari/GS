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

        private bool _DialogIsOn;
        public bool DialogIsOn
        {
            get
            {
                return _DialogIsOn;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _DialogIsOn, value);
            }
        }

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

            this.SignIn.WhenAnyValue(x => x.IsEnabled).Subscribe(x =>
            {
                if (InsideJob)
                {

                    return;
                }
                this.RefreshSignInSwitch = !this.RefreshSignInSwitch;

                if (x == false && App.IsRegistered)
                {
                    this.WarnCommand.Execute(true);
                    this.DialogIsOn = true;

                }
                if (x == true && !App.IsRegistered)
                    this.Navigate(new SignInRegisterViewModel(App));
            });


            App.BackKeyPressedCommand.OfType<CancelEventArgs>().Subscribe(x =>
             {
                 if (this.DialogIsOn)
                 {
                     this.WarnCommand.Execute(false);
                     this.DialogIsOn = false;
                     this.RefreshSignInSwitch = !this.RefreshSignInSwitch;
                     x.Cancel = true;
                 }

             });



            var logOutResult = this.SignOutCommand.RegisterAsyncTask(async (_) => await App.SignOut());
            logOutResult.Subscribe(x => this.Navigate(new MainViewModel(App)));

            this.SharedByDefault = new ButtonViewModel()
            {
                IsEnabled = false,

            };
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
