using Growthstories.UI.ViewModel;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace GrowthStories.UI.WindowsPhone
{
    public static class ViewModelExtensions
    {


        private static PhoneApplicationFrame _mainFrame;

        public static void GoBack(this GSViewModelBase VM)
        {
            if (EnsureMainFrame()
                && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        public static void NavigateTo(this GSViewModelBase VM, Uri pageUri)
        {
            if (EnsureMainFrame())
            {
                _mainFrame.Navigate(pageUri);
            }
        }

        private static bool EnsureMainFrame()
        {
            if (_mainFrame != null)
            {
                return true;
            }

            _mainFrame = App.Current.RootVisual as PhoneApplicationFrame;

            if (_mainFrame != null)
            {
                return true;
            }

            return false;
        }


        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand), typeof(ViewModelExtensions),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(LongListSelector selector)
        {
            return (ICommand)selector.GetValue(CommandProperty);
        }

        public static void SetCommand(LongListSelector selector, ICommand value)
        {
            selector.SetValue(CommandProperty, value);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as LongListSelector;
            if (selector == null)
            {
                throw new ArgumentException(
                    "You must set the Command attached property on an element that derives from LongListSelector.");
            }

            var oldCommand = e.OldValue as ICommand;
            if (oldCommand != null)
            {
                selector.SelectionChanged -= OnSelectionChanged;
            }

            var newCommand = e.NewValue as ICommand;
            if (newCommand != null)
            {
                selector.SelectionChanged += OnSelectionChanged;
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selector = sender as LongListSelector;
            var command = GetCommand(selector);

            if (command != null && selector.SelectedItem != null)
            {
                command.Execute(selector.SelectedItem);
            }

            selector.SelectedItem = null;
        }


    }
}
