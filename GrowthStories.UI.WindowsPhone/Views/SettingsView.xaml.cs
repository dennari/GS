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
using System.Reactive.Linq;


namespace Growthstories.UI.WindowsPhone
{
    public class SettingsViewBase : GSView<ISettingsViewModel>
    {

    }

    public partial class SettingsView : SettingsViewBase
    {

        public SettingsView()
        {
            InitializeComponent();
        }

        protected override void OnViewModelChanged(ISettingsViewModel vm)
        {
            base.OnViewModelChanged(vm);
        }

        //    IDisposable WarnSubscription = Disposable.Empty;
        //    protected override void OnViewModelChanged(ISettingsViewModel vm)
        //    {
        //        WarnSubscription.Dispose();
        //        WarnSubscription = vm.WarnCommand
        //            .OfType<bool>()
        //            .ObserveOn(RxApp.MainThreadScheduler)
        //            .Subscribe(x =>
        //            {
        //                if (x)
        //                    this.LogOutDialog.Show();
        //                else
        //                    this.LogOutDialog.Dismiss();
        //            });
        //    }


        //    CustomMessageBox _LogOutDialog;
        //    CustomMessageBox LogOutDialog
        //    {
        //        get
        //        {
        //            if (_LogOutDialog == null || _LogOutDialog != null)
        //            {
        //                _LogOutDialog = new CustomMessageBox()
        //                {
        //                    Caption = "Confirmation",
        //                    Message = "Are you sure you wish to sign out?",
        //                    LeftButtonContent = "yes",
        //                    IsRightButtonEnabled = false
        //                };

        //                _LogOutDialog.Dismissed += (s1, e1) =>
        //                {
        //                    switch (e1.Result)
        //                    {
        //                        case CustomMessageBoxResult.LeftButton:
        //                            ViewModel.SignOutCommand.Execute(null);
        //                            break;
        //                        default:
        //                            break;
        //                    }
        //                    ViewModel.DialogIsOn = false;

        //                };

        //            }
        //            return _LogOutDialog;
        //        }
        //    }
        //}

    }

}