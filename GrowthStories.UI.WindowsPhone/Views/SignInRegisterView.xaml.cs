using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Growthstories.UI.ViewModel;

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
            if (vm == null)
                return;
            // unfocus any textbox when submitted
            vm.OKCommand.Subscribe(_ => this.Focus());
        }

        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_LostFocus();
            ViewModel.UsernameTouched = true;
        }

        private void email_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_LostFocus();
            ViewModel.EmailTouched = true;
        }

        private void password_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_LostFocus();
            ViewModel.PasswordTouched = true;
        }

        private void passwordConfirmation_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_LostFocus();
            ViewModel.PasswordConfirmationTouched = true;
        }

        private void TextBox_LostFocus()
        {
            SIPHelper.SIPGotHidden(SIPPlaceHolder);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SIPHelper.SIPGotVisible(SIPPlaceHolder);
        }


        //private void GSViewGrid_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (!ViewModel.App.HasDataConnection)
        //    {
        //        string msg;

        //        if (ViewModel.SignInMode)
        //        {
        //            msg = "Signing in requires a data connection. Please enable one in your phone's settings and try again";

        //        }
        //        else
        //        {
        //            msg = "Registration requires a data connection. Please enable one in your phone's settings and try again";

        //        }

        //        PopupViewModel pvm = new PopupViewModel()
        //        {
        //            Caption = "Data connection required",
        //            Message = msg,
        //            IsLeftButtonEnabled = true,
        //            DismissedCommand = ViewModel.App.Router.NavigateBack,
        //            LeftButtonContent = "OK"
        //        };
        //        ViewModel.App.ShowPopup.Execute(pvm);
        //    } 
        //}


    }


}