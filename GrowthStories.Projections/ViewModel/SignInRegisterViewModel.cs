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

namespace Growthstories.UI.ViewModel
{




    public class SignInRegisterViewModel : RoutableViewModel, ISignInRegisterViewModel
    {


        public IReactiveCommand OKCommand { get; protected set; }
        public IReactiveCommand SwitchModeCommand { get; protected set; }
        public IObservable<Tuple<bool, RegisterResponse, SignInResponse>> Response { get; protected set; }


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
                this.ProgressIndicatorIsVisible = true;
            });
            this.OKCommand.ThrownExceptions.Subscribe(x =>
            {
                throw x;
            });

            this.Response = this.OKCommand.RegisterAsyncTask(async _ =>
            {
                if (SignInMode)
                {
                    var r = await App.SignIn(this.Email, this.Password);
                    return Tuple.Create(true, RegisterResponse.alreadyRegistered, r);

                }
                else
                {
                    var r = await App.Register(this.Username, this.Email, this.Password);
                    return Tuple.Create(false, r, SignInResponse.accountNotFound);
                }
            });
            this.Response.Subscribe(x =>
            {
                this.ProgressIndicatorIsVisible = false;
                bool IsSuccess = x.Item1 ? x.Item3 == SignInResponse.success : x.Item2 == RegisterResponse.success;

                if (x.Item2 == RegisterResponse.emailInUse)
                {
                    this.Message = "The specified email-address is already registered";
                }

                if (IsSuccess)
                {
                    if (!SignInMode)
                        App.Router.NavigateBack.Execute(null);
                    else
                        App.Router.NavigateAndReset.Execute(new MainViewModel(App));

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


            this.WhenAnyValue(x => x.SignInMode).Subscribe(x => this.Title = !x ? "new user" : "sign in");





        }

        bool EmailCheck(string email)
        {
            return email != null && email.Length > 4;
        }

        bool UsernameCheck(string username)
        {
            return SignInMode || (username != null && username.Length > 2);
        }

        bool PasswordCheck(string p, string pc)
        {
            return p != null && p.Length >= 6 && (SignInMode || p == pc);
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

        private bool _SignInMode = true;
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



}
