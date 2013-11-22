using Microsoft.Phone.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Growthstories.UI.WindowsPhone
{
    public class GSView<T> : UserControl, IViewFor<T> where T : class
    {
        public GSView()
        {
            //this.SetBinding(ViewModelProperty, new Binding());
        }

        public T ViewModel
        {
            get { return (T)GetValue(ViewModelProperty); }
            set
            {
                if (value != null && value != ViewModel)
                {
                    SetValue(ViewModelProperty, value);
                    OnViewModelChanged(value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(GSView<T>), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        object IViewFor.ViewModel
        {
            get
            {
                return this.ViewModel;
            }
            set
            {
                if (value != null && value != ViewModel)
                {
                    var vm = value as T;
                    if (vm != null)
                        this.ViewModel = vm;
                }
            }
        }

        protected virtual void OnViewModelChanged(T vm)
        {


        }

    }

    public class GSContentControl<T> : ContentControl, IViewFor<T> where T : class
    {
        public GSContentControl()
        {
            //this.SetBinding(ViewModelProperty, new Binding());
        }

        public T ViewModel
        {
            get { return (T)GetValue(ViewModelProperty); }
            set
            {
                if (value != null && value != ViewModel)
                {
                    SetValue(ViewModelProperty, value);
                    OnViewModelChanged(value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(GSContentControl<T>), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        object IViewFor.ViewModel
        {
            get
            {
                return this.ViewModel;
            }
            set
            {
                if (value != null && value != ViewModel)
                {
                    var vm = value as T;
                    if (vm != null)
                        this.ViewModel = vm;
                }
            }
        }

        protected virtual void OnViewModelChanged(T vm)
        {


        }

    }

    public class GSPage<T> : PhoneApplicationPage, IViewFor<T> where T : class
    {
        public GSPage()
        {
            this.SetBinding(ViewModelProperty, new Binding());
        }

        public T ViewModel
        {
            get { return (T)GetValue(ViewModelProperty); }
            set
            {
                if (value != null && value != ViewModel)
                {
                    SetValue(ViewModelProperty, value);
                    OnViewModelChanged(value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(GSPage<T>), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        object IViewFor.ViewModel
        {
            get
            {
                return this.ViewModel;
            }
            set
            {
                if (value != null && value != ViewModel)
                {
                    var vm = value as T;
                    if (vm != null)
                        this.ViewModel = vm;
                }
            }
        }

        protected virtual void OnViewModelChanged(T vm)
        {


        }

    }
}
