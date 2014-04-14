using System;
using System.Windows.Controls;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using Telerik.Windows.Controls.PhoneTextBox;

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

            if (vm == null)
                return;

            var box = UserListBox;

            vm.WhenAnyValue(x => x.ProgressIndicatorIsVisible).Subscribe(x =>
            {
                if (x)
                {
                    UserListBox.ChangeValidationState(ValidationState.Validating, "Searching for users");
                }
                else
                {
                    UserListBox.ChangeValidationState(ValidationState.NotValidated, "");
                }
            });

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


        private void UserListBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            UserSelector.Height = 560 - SIPHelper.GetSipHeight();
        }


        private void UserListBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            UserSelector.Height = 560;
        }

        private void UserListBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UserListBox.Text = "";
            this.ViewModel.Search = null;

        }


    }
}