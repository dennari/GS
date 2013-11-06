using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Growthstories.UI.WindowsPhone
{

    public interface IReactsToViewModelChange
    {
        void ViewModelChanged(object vm);

    }

    public static class ViewHelpers
    {


        public static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (FrameworkElement)sender;
                var vm = e.NewValue;
                if (view.DataContext == vm)
                    return;
                view.DataContext = vm;


                var viewI = view as IReactsToViewModelChange;
                if (viewI != null)
                    viewI.ViewModelChanged(vm);
            }
            catch { }
        }
    }
}
