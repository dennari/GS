﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Growthstories.UI.ViewModel;
using Growthstories.UI.WindowsPhone;
using Growthstories.UI.WindowsPhone.ViewModels;
using ReactiveUI;
using System.Windows.Input;

namespace Growthstories.UI.WindowsPhone
{
    public partial class ListUsersView : UserControl, IViewFor<ListUsersViewModel>
    {
        public ListUsersView()
        {
            InitializeComponent();

        }

        public ListUsersViewModel ViewModel
        {
            get { return (ListUsersViewModel)GetValue(ViewModelProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                    this.DataContext = value;
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(ListUsersView), new PropertyMetadata(null));


        object IViewFor.ViewModel { get { return this.ViewModel; } set { this.ViewModel = (ListUsersViewModel)value; } }



        private void UserListBox_IconTapped(object sender, EventArgs e)
        {

            UserListBox.Text = null;
            this.Focus();

        }

        private void UserListBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.ViewModel.SearchCommand.Execute(UserListBox.Text);
        }


    }
}