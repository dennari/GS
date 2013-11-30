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
using ReactiveUI;
using System.Windows.Input;
using Growthstories.Sync;
using System.Reactive.Disposables;

namespace Growthstories.UI.WindowsPhone
{
    public class ListUsersViewBase : GSView<ISearchUsersViewModel>
    {

    }

    public partial class ListUsersView : ListUsersViewBase
    {

        public ListUsersView()
        {
            InitializeComponent();
            UserSelector.SelectedItem = null;
        }



        private void UserListBox_IconTapped(object sender, EventArgs e)
        {

            UserListBox.Text = null;
            UserSelector.SelectedItem = null;
            this.Focus();

        }

        private void UserListBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.ViewModel.Search = UserListBox.Text;
            this.ViewModel.SearchCommand.Execute(UserListBox.Text);
        }

        private void UserSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var item = UserSelector.SelectedItem;
            if (item == null || !(item is RemoteUser))
                return;

            ViewModel.UserSelectedCommand.Execute(item);
            UserSelector.SelectedItem = null;
        }

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }


    }
}