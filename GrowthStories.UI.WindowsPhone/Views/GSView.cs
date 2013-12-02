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

    public interface IReportViewModelChange
    {
        void ViewModelChangeReport(object vm);
    }

    public class GSView<T> : UserControl, IReportViewModelChange, IViewFor<T> where T : class
    {
        public GSView()
        {
            this.SetBinding(ViewModelProperty, new Binding());
            Height = 800;
        }

        public T ViewModel
        {
            get { return GetValue(ViewModelProperty) as T; }
            set
            {
                if (value != null)
                {
                    SetValue(ViewModelProperty, value);
                    //OnViewModelChanged(value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(UserControl), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        object IViewFor.ViewModel
        {
            get
            {
                //return this.GetViewModel();
                return this.ViewModel;
            }
            set
            {

                this.ViewModel = value as T;
            }
        }

        public void ViewModelChangeReport(object vm)
        {
            if (vm == null)
                return;
            var v = vm as T;
            if (v == null)
                return;
            if (vm != this.ViewModel)
                this.ViewModel = v;
            this.OnViewModelChanged(v);
        }

        protected virtual void OnViewModelChanged(T vm)
        {


        }

    }





    public class GSContentControl<T> : ContentControl, IReportViewModelChange, IViewFor<T> where T : class
    {
        public GSContentControl()
        {
            //this.SetBinding(ViewModelProperty, new Binding());
            //this.WhenAny()
        }

        //public static readonly DependencyProperty ViewModelProperty =
        //    DependencyProperty.Register("ViewModel", typeof(T), typeof(ContentControl), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(T), typeof(ContentControl), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));

        public T ViewModel
        {
            get
            {
                return GetValue(ViewModelProperty) as T;
            }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }


        object IViewFor.ViewModel
        {
            get
            {
                return this.ViewModel;
            }
            set
            {
                this.ViewModel = value as T;
            }
        }

        protected virtual void OnViewModelChanged(T vm)
        {


        }

        public void ViewModelChangeReport(object vm)
        {

            this.OnViewModelChanged(vm as T);
        }

    }

    public class GSPage<T> : PhoneApplicationPage, IReportViewModelChange, IViewFor<T> where T : class
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
                    // OnViewModelChanged(value);
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(IRoutableViewModel), typeof(Page), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));




        object IViewFor.ViewModel
        {
            get
            {
                //return this.GetViewModel();
                return this.ViewModel;
            }
            set
            {

                this.ViewModel = value as T;
            }
        }

        protected virtual void OnViewModelChanged(T vm)
        {


        }

        public void ViewModelChangeReport(object vm)
        {
            if (vm == null)
                return;
            var v = vm as T;
            if (v == null)
                return;
            if (vm != this.ViewModel)
                this.ViewModel = v;
            this.OnViewModelChanged(v);
        }

    }
}
