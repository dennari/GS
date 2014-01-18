using System;
using System.Windows.Controls;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;

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


        protected override void OnViewModelChanged(ISearchUsersViewModel vm)
        {

        }


        private void UserListBox_IconTapped(object sender, EventArgs e)
        {

            UserListBox.Text = null;
            UserSelector.SelectedItem = null;
            this.Focus();

        }

        private void UserListBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
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


    }
}