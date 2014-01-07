using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Growthstories.UI;
using System.Collections.Generic;

namespace GrowthStories.UI.WindowsPhone
{
    public class NavigationService : INavigationService
    {
        private PhoneApplicationFrame _mainFrame;

        public void GoBack()
        {
            if (EnsureMainFrame()
                && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        readonly IDictionary<View, Uri> _ViewUri = new Dictionary<View, Uri>()
        {
            {View.GARDEN,new Uri("/Views/GardenView.xaml", UriKind.Relative)},
            {View.PLANT,new Uri("/Views/PlantView.xaml", UriKind.Relative)},
            {View.ADD_PLANT,new Uri("/Views/AddPlantView.xaml", UriKind.Relative)},
            {View.SELECT_PROFILE_PICTURE,new Uri("/Views/SelectProfilePictureView.xaml", UriKind.Relative)}
        };

        readonly IDictionary<IconType, Uri> _IconUri = new Dictionary<IconType, Uri>()
        {
            {IconType.ADD,new Uri("/Assets/Icons/appbar.add.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK,new Uri("/Assets/Icons/appbar.check.png", UriKind.RelativeOrAbsolute)},
            {IconType.DELETE,new Uri("/Assets/Icons/appbar.delete.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK_LIST,new Uri("/Assets/Icons/appbar.list.check.png", UriKind.RelativeOrAbsolute)}
        };

        public IDictionary<View, Uri> ViewUri { get { return _ViewUri; } }
        public IDictionary<IconType, Uri> IconUri { get { return _IconUri; } }


        public void NavigateTo(View view)
        {
            if (EnsureMainFrame())
            {
                _mainFrame.Navigate(ViewUri[view]);
            }
        }

        private bool EnsureMainFrame()
        {
            if (_mainFrame != null)
            {
                return true;
            }

            _mainFrame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (_mainFrame != null)
            {
                return true;
            }

            return false;
        }

    }
}