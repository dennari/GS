using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Microsoft.Phone.Tasks;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using Growthstories.UI.WindowsPhone.ViewModels;
using System.Reactive.Disposables;
using Telerik.Windows.Controls;

namespace Growthstories.UI.WindowsPhone
{
    public class SignInRegisterViewBase : GSView<ISignInRegisterViewModel>
    {

    }

    public partial class SignInRegisterView : SignInRegisterViewBase
    {

        public SignInRegisterView()
        {
            InitializeComponent();

            this.TabItems = new List<Control>()
            {
                this.username,
                this.email,
                this.password,
                this.passwordConfirmation
            };
        }

        protected override void OnViewModelChanged(ISignInRegisterViewModel vm)
        {
            base.OnViewModelChanged(vm);

            if (!vm.App.HasDataConnection)
            {
                string msg;

                if (vm.SignInMode) {
                    msg = "Signing in requires a data connection. Please enable one in your phone settings and try again";
                
                } else {
                    msg = "Signing up requires a data connection. Please enable one in your phone settings and try again";

                }

                PopupViewModel pvm = new PopupViewModel()
                {
                    Caption = "Data connection required",
                    Message = msg,
                    IsLeftButtonEnabled = true,
                    DismissedCommand = vm.App.Router.NavigateBack,
                    LeftButtonContent = "OK"
                };
                vm.App.ShowPopup.Execute(pvm);
            } 
        }

        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.UsernameTouched = true;
        }

        private void email_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.EmailTouched = true;
        }

        private void password_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.PasswordTouched = true;
        }

        private void passwordConfirmation_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.PasswordConfirmationTouched = true;
        }

    }


}