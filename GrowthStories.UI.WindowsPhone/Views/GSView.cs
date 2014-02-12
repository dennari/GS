using Microsoft.Phone.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EventStore.Logging;


namespace Growthstories.UI.WindowsPhone
{

    public interface IReportViewModelChange
    {
        void ViewModelChangeReport(object vm);
    }


    public class GSView<T> : UserControl, IReportViewModelChange, IViewFor<T> where T : class
    {

        private static ILog Logger = LogFactory.BuildLogger(typeof(GSView<T>));
        

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register(
            "ViewModel",
            typeof(IRoutableViewModel),
            typeof(UserControl),
            new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        public GSView()
        {

            // We don't want to set this as then we will have "two way" sync problems between ViewModel and DataContext
            // so in XAML, just set ViewModel={..} and both ViewModel and DataContext will be set
            // this.SetBinding(ViewModelProperty, new Binding());

            // this is needed to prevent shitty image resizing for
            // pages with backgrounds, affecting LUMIA 520 and probably 
            // other models
            //
            // it however breaks landscape views so in those we should
            // set Height = Double.NaN, which sets height to Auto
            //
            //  -- JOJ 22.12.2013
            if ((int)Math.Round(Height) != 800)
            {
                Height = 800;
            }

        }


        public T ViewModel
        {
            get { return GetValue(ViewModelProperty) as T; }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }



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

            if (vm != this.ViewModel) // this happens if set the ViewModel property in XAML
            {
                this.ViewModel = vm as T; // this triggers another ViewModelChangeReport
                return;
            }

            this.OnViewModelChanged(vm as T);
        }


        protected virtual void OnViewModelChanged(T vm)
        {


        }

        protected List<Control> TabItems { get; set; }


        public void NotifyDestroyed(String msg)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                var name = this.GetType().Name;
                System.Diagnostics.Debugger.Log(1, "dest", string.Format("DESTROYING {0},{1}\n", name, msg));
            }

            //var cmd = new ReactiveCommand();
            //cmd.ObserveOn(RxApp.TaskpoolScheduler).Subscribe(_ =>
            //{
            //    var name = this.GetType().Name;
            //    Logger.Info("DESTROYING {0},{1}", name, msg);
            //});
            //cmd.Execute(null);
        }


        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == System.Windows.Input.Key.Enter && TabItems != null)
            {
                var current = e.OriginalSource as Control;
                if (current != null)
                {
                    int i = 0;
                    int c = TabItems.Count;
                    bool sawSelf = false;
                    Control nextVisible = null;

                    while (i < c && nextVisible == null)
                    {
                        try
                        {
                            var candidate = TabItems[i];
                            if (candidate == current)
                                sawSelf = true;
                            else if (sawSelf && candidate.Visibility == System.Windows.Visibility.Visible)
                                nextVisible = candidate;
                        }
                        catch { }
                        i++;
                    }
                    if (nextVisible != null)
                        nextVisible.Focus();
                }
            }
        }
    }




    public class GSContentControl<T> : ContentControl, IReportViewModelChange, IViewFor<T> where T : class
    {
        public GSContentControl()
        {

        }


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

            if (vm != this.ViewModel) // this happens if set the ViewModel property in XAML
            {
                this.ViewModel = vm as T; // this triggers another ViewModelChangeReport
                return;
            }

            this.OnViewModelChanged(vm as T);
        }

    }

    public class GSPage<T> : PhoneApplicationPage, IReportViewModelChange, IViewFor<T> where T : class
    {
        public GSPage()
        {
            //this.SetBinding(ViewModelProperty, new Binding()); // TEST JUHO
        }

        public T ViewModel
        {
            get { return (T)GetValue(ViewModelProperty); }
            set
            {

                SetValue(ViewModelProperty, value);
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

            if (vm != this.ViewModel) // this happens if set the ViewModel property in XAML
            {
                this.ViewModel = vm as T; // this triggers another ViewModelChangeReport
                return;
            }

            this.OnViewModelChanged(vm as T);
        }

    }






    public class GSRoutedViewHost : ContentControl
    {
        //IDisposable _inner = null;

        /// <summary>
        /// The Router associated with this View Host.
        /// </summary>
        public IRoutingState Router
        {
            get { return (IRoutingState)GetValue(RouterProperty); }
            set { SetValue(RouterProperty, value); }
        }
        public static readonly DependencyProperty RouterProperty =
            DependencyProperty.Register("Router", typeof(IRoutingState), typeof(GSRoutedViewHost), new PropertyMetadata(null));

        /// <summary>
        /// This content is displayed whenever there is no page currently
        /// routed.
        /// </summary>
        public object DefaultContent
        {
            get { return (object)GetValue(DefaultContentProperty); }
            set { SetValue(DefaultContentProperty, value); }
        }
        public static readonly DependencyProperty DefaultContentProperty =
            DependencyProperty.Register("DefaultContent", typeof(object), typeof(GSRoutedViewHost), new PropertyMetadata(null));


        public IViewLocator ViewLocator { get; set; }

        public GSRoutedViewHost()
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;

            if (RxApp.InUnitTestRunner()) return;

            // NB: The DistinctUntilChanged is useful because most views in 
            // WinRT will end up getting here twice - once for configuring
            // the RoutedViewHost's ViewModel, and once on load via SizeChanged
            this.WhenAnyObservable(x => x.Router.CurrentViewModel).DistinctUntilChanged().Subscribe(x =>
            {
                if (x == null)
                {
                    Content = DefaultContent;
                    return;
                }

                var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                var view = viewLocator.ResolveView(x);

                if (view == null)
                {
                    throw new Exception(String.Format("Couldn't find view for '{0}'.", x));
                }

                view.ViewModel = x;
                Content = view;
            }, ex => RxApp.DefaultExceptionHandler.OnNext(ex));
        }
    }

}
