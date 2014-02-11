using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Controls;
using ReactiveUI.Xaml;
using ReactiveUI;
using System.Reactive.Linq;
using Growthstories.UI.ViewModel;
using AppViewModel = Growthstories.UI.WindowsPhone.ViewModels.ClientAppViewModel;
using System.Windows.Controls;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Disposables;
using EventStore.Logging;

namespace Growthstories.UI.WindowsPhone
{



    /// <summary>
    /// This control hosts the View associated with a Router, and will display
    /// the View and wire up the ViewModel whenever a new ViewModel is
    /// navigated to. Put this control as the only control in your Window.
    /// </summary>
    public class AGSRoutedViewHost : RadTransitionControl
    {
        IDisposable _inner = null;

        /// <summary>
        /// The Router associated with this View Host.
        /// </summary>
        public IRoutingState Router
        {
            get { return (IRoutingState)GetValue(RouterProperty); }
            set { SetValue(RouterProperty, value); }
        }
        public static readonly DependencyProperty RouterProperty =
            DependencyProperty.Register("Router", typeof(IRoutingState), typeof(AGSRoutedViewHost), new PropertyMetadata(null));

        public static readonly DependencyProperty AppVMProperty =
             DependencyProperty.Register("AppVM", typeof(AppViewModel), typeof(AGSRoutedViewHost), new PropertyMetadata(null));

        public AppViewModel AppVM
        {
            get { return (AppViewModel)GetValue(AppVMProperty); }
            set { SetValue(AppVMProperty, value); }
        }

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
            DependencyProperty.Register("DefaultContent", typeof(object), typeof(AGSRoutedViewHost), new PropertyMetadata(null));

        public IObservable<string> ViewContractObservable
        {
            get { return (IObservable<string>)GetValue(ViewContractObservableProperty); }
            set { SetValue(ViewContractObservableProperty, value); }
        }
        public static readonly DependencyProperty ViewContractObservableProperty =
            DependencyProperty.Register("ViewContractObservable", typeof(IObservable<string>), typeof(AGSRoutedViewHost), new PropertyMetadata(Observable.Return(default(string))));

        public IViewLocator ViewLocator { get; set; }

        private static ILog Logger = LogFactory.BuildLogger(typeof(AGSRoutedViewHost));


        public AGSRoutedViewHost()
        {
            
            

            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;

            if (RxApp.InUnitTestRunner())
            {
                ViewContractObservable = Observable.Never<string>();
                return;
            }

            var platform = RxApp.DependencyResolver.GetService<IPlatformOperations>();
            if (platform == null)
            {
                throw new Exception("Couldn't find an IPlatformOperations. This should never happen, your dependency resolver is broken");
            }

            ViewContractObservable = Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(x => SizeChanged += x, x => SizeChanged -= x)
                .Select(_ => platform.GetOrientation())
                .DistinctUntilChanged()
                .StartWith(platform.GetOrientation())
                .Select(x => x != null ? x.ToString() : default(string));

            //var vmAndContract = Observable.CombineLatest(
            //    this.WhenAnyObservable(x => x.Router.CurrentViewModel),
            //    this.WhenAnyObservable(x => x.ViewContractObservable),
            //    (vm, contract) => Tuple.Create(vm, contract));

            // NB: The DistinctUntilChanged is useful because most views in 
            // WinRT will end up getting here twice - once for configuring
            // the RoutedViewHost's ViewModel, and once on load via SizeChanged
            this.WhenAnyObservable(x => x.Router.CurrentViewModel).DistinctUntilChanged().Subscribe(x =>
            {

                this.IsBackTransition = AppVM != null && AppVM.NavigatingBack;
                if (x == null)
                {
                    Content = DefaultContent;
                    if (AppVM != null)
                    {
                        AppVM.NavigatingBack = false;
                    }
                    return;
                }
                
                var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                var view = viewLocator.ResolveView(x, null);

                if (view == null)
                {
                    throw new Exception(String.Format("Couldn't find view for '{0}'.", x));
                }
                view.ViewModel = x;
                try
                {
                    Content = view;
                }
                catch (Exception e)
                {
                    Logger.Warn("could not set content for viewModel {0}, view {1}", view.ViewModel, view); 
                }
                
                if (AppVM != null)
                {
                    AppVM.NavigatingBack = false;
                }

            }, ex => RxApp.DefaultExceptionHandler.OnNext(ex));
        }
    }

    /// <summary>
    /// This content control will automatically load the View associated with
    /// the ViewModel property and display it. This control is very useful
    /// inside a DataTemplate to display the View associated with a ViewModel.
    /// </summary>
    public class GSMultiViewHost : ContentControl, IReportViewModelChange
    {
        /// <summary>
        /// The ViewModel to display
        /// </summary>
        public IHasInnerViewModel ViewModel
        {
            get { return (IHasInnerViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IHasInnerViewModel), typeof(GSMultiViewHost), new PropertyMetadata(null, ViewHelpers.ViewModelValueChanged));


        /// <summary>
        /// If no ViewModel is displayed, this content (i.e. a control) will be displayed.
        /// </summary>
        public FrameworkElement DefaultContent
        {
            get { return (FrameworkElement)GetValue(DefaultContentProperty); }
            set { SetValue(DefaultContentProperty, value); }
        }
        public static readonly DependencyProperty DefaultContentProperty =
            DependencyProperty.Register("DefaultContent", typeof(FrameworkElement), typeof(GSMultiViewHost), new PropertyMetadata(null, OnDefaultContentChanged));

        private static void OnDefaultContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var This = (GSMultiViewHost)d;
            var v = (FrameworkElement)e.NewValue;
            if (This.DefaultContent != v)
                This.DefaultContent = v;

            if (This.Content == null)
                This.Content = v;
        }


        public void ViewModelChangeReport(object vm)
        {

            if (vm != this.ViewModel) // this happens if set the ViewModel property in XAML
            {
                this.ViewModel = vm as IHasInnerViewModel; // this triggers another ViewModelChangeReport
                return;
            }

            this.OnViewModelChanged(vm as IHasInnerViewModel);
        }

        private IDisposable subscription = Disposable.Empty;
        protected virtual void OnViewModelChanged(IHasInnerViewModel vm)
        {

            subscription.Dispose();
            subscription = vm.WhenAnyValue(x => x.InnerViewModel).Subscribe(x =>
            {
                if (x == null)
                {
                    Content = null;
                    Content = DefaultContent;
                    return;
                }

                var view = ViewLocator.ResolveView(x, null);

                if (view == null)
                {
                    throw new Exception(String.Format("Couldn't find view for '{0}'.", x));
                }

                view.ViewModel = x;
                Content = view;
            });

        }

        private IViewLocator ViewLocator;

        public GSMultiViewHost()
        {

            ViewLocator = ReactiveUI.ViewLocator.Current;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            //Content = DefaultContent;


        }

        //protected override 



    }

}

