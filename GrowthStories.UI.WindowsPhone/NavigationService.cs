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

        public readonly IDictionary<View, Uri> ViewUri = new Dictionary<View, Uri>()
        {
            {View.GARDEN,new Uri("/Views/GardenView.xaml", UriKind.Relative)},
            {View.PLANT,new Uri("/Views/PlantView.xaml", UriKind.Relative)},
            {View.ADD_PLANT,new Uri("/Views/AddPlantView.xaml", UriKind.Relative)}
        };

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