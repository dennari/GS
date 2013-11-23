using Growthstories.UI.ViewModel;
using Microsoft.Phone.Controls;
using ReactiveUI;
using System;
using System.Windows;


namespace Growthstories.UI.WindowsPhone
{


    public class SignInRegisterView : GSContentControl<ISignInRegisterViewModel>
    {




        protected override void OnViewModelChanged(ISignInRegisterViewModel vm)
        {

            if (vm == null)
                return;

            this.ContentTemplate = (vm.IsRegistered ? Application.Current.Resources["SigninTemplate"] : Application.Current.Resources["RegisterTemplate"]) as DataTemplate;
            this.Content = vm;

        }



        public SignInRegisterView()
        {

        }



    }



}
