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




    public class SignInRegisterViewModel : RoutableViewModel, ISignInRegisterViewModel, IControlsAppBar
    {


        public IReactiveCommand OKCommand { get; protected set; }

        public SignInRegisterViewModel(IGSAppViewModel app)
            : base(app)
        {


            var canExecute = this.WhenAnyValue(
                    x => x.Email,
                    x => x.Password,
                    x => x.PasswordConfirmation,
                    (e, p, pc) =>
                    {

                        return this.EmailCheck(e) && this.PasswordCheck(p, pc);

                    }
                );
            canExecute.Subscribe(x => this.CanExecute = x);

            this.OKCommand = new ReactiveCommand(canExecute);

            this.IsRegistered = app.User.IsRegistered();

            this.Title = IsRegistered ? "New user" : "Sign in";
            this.LeftButtonContent = "submit";

            //app.WhenAnyValue(x => x.User.)


        }

        bool EmailCheck(string email)
        {
            return email != null && email.Length > 4;
        }

        bool PasswordCheck(string p, string pc)
        {
            return p != null && p.Length >= 6 && (IsRegistered || p == pc);
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
    }
}
