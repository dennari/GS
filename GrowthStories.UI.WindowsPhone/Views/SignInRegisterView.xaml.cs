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


        }
    }


}