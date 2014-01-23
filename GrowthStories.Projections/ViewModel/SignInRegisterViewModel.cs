using System;
using System.Reactive.Linq;
using System.Text;
using EventStore.Logging;
using PWDTK_MOBILE_WP_8;
using ReactiveUI;
using System.Text.RegularExpressions;


namespace Growthstories.UI.ViewModel
{


    public class SignInRegisterViewModel : RoutableViewModel, ISignInRegisterViewModel
    {


        private static ILog Logger = LogFactory.BuildLogger(typeof(SignInRegisterViewModel));

        public IReactiveCommand OKCommand { get; protected set; }
        public IReactiveCommand SwitchModeCommand { get; protected set; }
        public IObservable<Tuple<bool, RegisterResponse, SignInResponse>> Response { get; protected set; }

        public static bool IsSuccess(Tuple<bool, RegisterResponse, SignInResponse> x)
        {
            return x.Item1 ? x.Item3 == SignInResponse.success : x.Item2 == RegisterResponse.success;
        }

        private bool OperationFinished;

        public SignInRegisterViewModel(IGSAppViewModel app)
            : base(app)
        {

            var canExecute = this.WhenAnyValue(
                    x => x.Email,
                    x => x.Username,
                    x => x.Password,
                    x => x.PasswordConfirmation,
                    (e, u, p, pc) =>
                    {
                        this.Message = null;
                        return this.EmailCheck(e) && this.UsernameCheck(u) && this.PasswordCheck(p, pc);
                    }
                );
            canExecute.Subscribe(x => this.CanExecute = x);

            this.OKCommand = new ReactiveCommand(canExecute);
            this.OKCommand.Subscribe(x =>
            {
                this.Message = null;
                App.ShowPopup.Execute(PPViewModel());
            });

            var OKResponse = this.OKCommand.RegisterAsyncTask(async _ =>
            {
                OperationFinished = false;
                if (SignInMode)
                {
                    var r = await App.SignIn(this.Email, HashedPassword());
                    OperationFinished = true;
                    return Tuple.Create(true, RegisterResponse.alreadyRegistered, r);
                }
                else
                {
                    var r = await App.Register(this.Username, this.Email, HashedPassword());
                    OperationFinished = true;
                    return Tuple.Create(false, r, SignInResponse.invalidEmail);
                }
            });
            this.Response = OKResponse;

            this.Response.Subscribe(x =>
            {
                if (SignInMode && !App.SignInCancelRequested
                    || !SignInMode && !App.RegisterCancelRequested)
                {
                    App.ShowPopup.Execute(null);
                }
                
                if (IsSuccess(x))
                {
                    //if (SignInMode)
                    //    App.Router.NavigateAndReset.Execute(new MainViewModel(App));
                    //if (!SignInMode && NavigateBack)
                    //    App.Router.NavigateBack.Execute(null);
                }
                else
                {
                    var p = GetPopup(x);
                    if (p != null)
                    {
                        App.ShowPopup.Execute(p);
                    }

                    if (x.Item3 == SignInResponse.messCreated)
                    {
                        this.Log().Info("signing out after messy signin");
                        App.SignOut();
                    }
                }
            });


            this.SwitchModeCommand = new ReactiveCommand();
            this.SwitchModeCommand.Subscribe(_ =>
            {
                this.SignInMode = !this.SignInMode;
            });

            Observable.CombineLatest(
                this.WhenAnyValue(x => x.SignInMode),
                App.WhenAnyValue(x => x.User),
                (a, b) => Tuple.Create(a, b)
              ).Subscribe(x =>
              {
                  //this.IsRegistered = x.Item2 != null && x.Item1 == x.Item2.Id;
              });

            this.WhenAnyValue(x => x.SignInMode).Subscribe(x => this.Title = !x ? "register" : "sign in");
            // NavigateBack = true;

            DismissAllowed = true;
            App.SetDismissPopupAllowed.OfType<bool>().Subscribe(x =>
            {
                DismissAllowed = x;
            });

        }

        private bool DismissAllowed = false;



        private IPopupViewModel _NoConnectionAlert;
        public IPopupViewModel NoConnectionAlert
        {
            get
            {
                if (_NoConnectionAlert == null)
                {

                    var signInMsg = "Signing in requires a data connection. Please enable one in your phone's settings and try again";
                    var registerMsg = "Registration requires a data connection. Please enable one in your phone's settings and try again";

                    var popup = new PopupViewModel()
                    {
                        Caption = "No data connection available",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK"
                    };

                    popup.DismissedObservable.Take(1).Select(_ => new object()).Subscribe(_ =>
                    {
                        if (App.Router.NavigationStack.Count > 0)
                        {
                            App.Router.NavigateBack.Execute(null);
                        }
                    });
                
                    this.WhenAnyValue(x => x.SignInMode).Subscribe(x =>
                    {
                        popup.Message = x ? signInMsg : registerMsg;
                    });

                    _NoConnectionAlert = popup;


                }
                return _NoConnectionAlert;

            }
        }


        // from        
        // http://stackoverflow.com/questions/16167983/best-regular-expression-for-email-validation-in-c-sharp
        //
        public static bool ValidEmail(string email)
        {
            return Regex.IsMatch(email.ToLower(), @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
        }


        private IPopupViewModel GetPopup(Tuple<bool, RegisterResponse, SignInResponse> x)
        {
            string msg = null;
            string caption = null;

            if (!SignInMode)
            {

                caption = "Could not register";
                switch (x.Item2)
                {
                    case RegisterResponse.connectionerror:
                        msg = "We could could not create an account for you, because we could not reach the Growth Stories servers. Please try again later.";
                        break;

                    case RegisterResponse.emailInUse:
                        msg = "Could not create a new account for you, because the email address you provided is already in use.";
                        break;

                    case RegisterResponse.usernameInUse:
                        msg = "Could not create a new account for you, because the username you provided is already in use.";
                        break;

                    case RegisterResponse.canceled:
                        this.Log().Info("Register() returned registration canceled");
                        return null;
                }

            }
            else
            {
                caption = "Could not sign you in";
                switch (x.Item3)
                {
                    case SignInResponse.canceled:
                        // are are showing this right away
                        this.Log().Info("SignIn() returned signin canceled");
                        return null;

                    case SignInResponse.connectionerror:
                        msg = "We could not sign you in, because we could not reach the Growth Stories servers. Please try again later.";
                        break;

                    case SignInResponse.invalidEmail:
                        msg = "The email address was incorrect. Please check your input and try again.";
                        break;

                    case SignInResponse.invalidPassword:
                        msg = "The password was incorrect. Please check your input and try again.";
                        break;

                    case SignInResponse.messCreated:
                        msg = "We could not sign you in, because we could not could not obtain all data from the Growth Stories servers. Please try again later.";
                        break;
                }
            }

            var pvm = new PopupViewModel()
            {
                Caption = caption,
                Message = msg,
                LeftButtonContent = "OK"
            };

            return pvm;
        }


        private String HashedPassword()
        {
            // we don't use a proper salt, as we don't have one
            // easily available. we could use the user guid, but
            // this would require first requesting the guid from
            // the server with help of the email address
            //
            // we will not bother using salts here as salts are 
            // mainly useful when the whole database is leaked,
            // and we do separate hashing on the server
            // 
            var salt = "GSCRAZYSALTWHOA";
            var sb = Encoding.UTF8.GetBytes(salt);

            var bytes = PWDTK.PasswordToHash(sb, Password, 5000);

            return Convert.ToBase64String(bytes);
        }


        private ProgressPopupViewModel PPViewModel()
        {
            if (SignInMode)
            {
                var pvm = new ProgressPopupViewModel()
                {
                    Caption = "Signing in",
                    ProgressMessage = "Please wait while Growth Stories signs you in and downloads your data."
                    // dismissal of dialog on back button should only happen
                    // once we cannot cancel anymore
                    // DismissOnBackButton = false
                };

                pvm.DismissedObservable.Subscribe(_ =>
                {
                    App.SignInCancelRequested = true;
                    if (!OperationFinished && DismissAllowed)
                    {
                        var p = new PopupViewModel()
                        {
                            Caption = "Signin canceled",
                            Message = "You were not signed in. You can submit the form again anytime.",
                            IsLeftButtonEnabled = true,
                            LeftButtonContent = "OK"
                        };
                        App.ShowPopup.Execute(p);
                    }
                });

                return pvm;
            
            } else {
                var pvm = new ProgressPopupViewModel()
                {
                    Caption = "Registering",
                    ProgressMessage = "Please wait while Growth Stories creates you a new account.",
                };
                pvm.DismissedObservable.Subscribe(_ =>
                {
                    // if dismissed fires only after registration 
                    // it is already too late and this is ignored
                    App.RegisterCancelRequested = true;

                    if (!OperationFinished && DismissAllowed)
                    {
                        var p = new PopupViewModel()
                        {
                            Caption = "Registration canceled",
                            Message = "We did not create an account for you. You can submit the form again anytime.",
                            IsLeftButtonEnabled = true,
                            LeftButtonContent = "OK"
                        };
                        App.ShowPopup.Execute(p);
                    }
                });
                return pvm;
            }
        }


        bool EmailCheck(string email)
        {
            return email != null && ValidEmail(email);
        }

        bool UsernameCheck(string username)
        {
            return SignInMode || (username != null && username.Length > 2);
        }

        bool PasswordCheck(string p, string pc)
        {
            return p != null && p.Length >= 6 && (SignInMode || p == pc);
        }


        public bool _UsernameTouched;
        public bool UsernameTouched
        {
            get
            {
                return _UsernameTouched;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _UsernameTouched, value);
                this.raisePropertyChanged("UsernameComplaint");
            }
        }

        public bool _EmailTouched;
        public bool EmailTouched
        {
            get
            {
                return _EmailTouched;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _EmailTouched, value);
                this.raisePropertyChanged("EmailComplaint");
            }
        }

        public bool _PasswordTouched;
        public bool PasswordTouched
        {
            get
            {
                return _PasswordTouched;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _PasswordTouched, value);
                this.raisePropertyChanged("PasswordComplaint");
            }
        }


        public bool _PasswordConfirmationTouched;
        public bool PasswordConfirmationTouched
        {
            get
            {
                return _PasswordConfirmationTouched;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _PasswordConfirmationTouched, value);
                this.raisePropertyChanged("PasswordConfirmationComplaint");
            }
        }


        public string UsernameComplaint
        {
            get
            {
                if (!UsernameTouched)
                {
                    return null;
                }

                if (Username == null || Username.Length == 0)
                {
                    return "Username must be provided.";
                }

                if (!UsernameCheck(Username))
                {
                    return "Username is not valid. It should have at least three characters.";
                }

                return null;
            }
        }

        public string EmailComplaint
        {
            get
            {
                if (!EmailTouched)
                {
                    return null;
                }

                if (Email == null || Email.Length == 0)
                {
                    return "Email address must be provided.";
                }

                if (!EmailCheck(Email))
                {
                    return "Email address is not valid.";
                }

                return null;
            }
        }


        public string PasswordComplaint
        {
            get
            {
                if (!PasswordTouched)
                {
                    return null;
                }

                if (Password == null || Password.Length == 0)
                {
                    return "Password must be provided.";
                }

                if (Password.Length < 6)
                {
                    return "Password is not valid. It should have at least 6 characters.";
                }

                return null;
            }
        }

        public string PasswordConfirmationComplaint
        {
            get
            {
                if (!PasswordConfirmationTouched)
                {
                    return null;
                }

                if (PasswordConfirmation == null || PasswordConfirmation.Length == 0)
                {
                    return "Password confirmation must be provided.";
                }

                if (!PasswordConfirmation.Equals(Password))
                {
                    return "Password confirmation does not match password.";
                }

                return null;
            }
        }



        public bool PasswordComplainNoMatch()
        {
            return Password != null
                && Password.Length > 0
                && PasswordConfirmation != null
                && Password.Length > 0
                && !Password.Equals(PasswordConfirmation);
        }

        protected bool _CanExecute;
        public bool CanExecute
        {
            get
            {
                return _CanExecute;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _CanExecute, value);
            }
        }

        protected bool _IsRegistered;
        public bool IsRegistered
        {
            get
            {
                return _IsRegistered;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsRegistered, value);
            }
        }

        private bool _SignInMode = false;
        public bool SignInMode
        {
            get
            {
                return _SignInMode;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SignInMode, value);
            }
        }


        protected string _Username;
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Username, value);
                this.raisePropertyChanged("UsernameComplaint");
            }
        }

        protected string _Email;
        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Email, value);
                this.raisePropertyChanged("EmailComplaint");

            }
        }

        protected string _Message;
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Message, value);
            }
        }

        protected string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Title, value);
            }
        }

        protected string _LeftButtonContent;
        public string LeftButtonContent
        {
            get
            {
                return _LeftButtonContent;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LeftButtonContent, value);
            }
        }


        protected string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Password, value);
                this.raisePropertyChanged("PasswordComplaint");
            }
        }

        protected string _PasswordConfirmation;
        public string PasswordConfirmation
        {
            get
            {
                return _PasswordConfirmation;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PasswordConfirmation, value);
                this.raisePropertyChanged("PasswordConfirmationComplaint");
            }
        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
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

        ReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons ?? (_AppBarButtons = new ReactiveList<IButtonViewModel>()
                {
                    new ButtonViewModel() {
                        Text = "submit",
                        IconType = IconType.CHECK,
                        Command = this.OKCommand
                    }
                });
            }
        }


    }


}
