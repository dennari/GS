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



    public static class ViewHelpers
    {



        public static void ViewModelValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var view = (FrameworkElement)sender;
                var vm = e.NewValue;

                if (view.DataContext != vm)
                    view.DataContext = vm;

                var viewF = view as IViewFor;
                if (viewF != null && viewF.ViewModel != vm)
                    viewF.ViewModel = vm;


            }
            catch { }
        }
    }
}
