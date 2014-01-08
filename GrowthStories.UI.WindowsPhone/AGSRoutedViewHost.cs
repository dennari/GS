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

using AppViewModel = Growthstories.UI.WindowsPhone.ViewModels.AppViewModel;

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

            var vmAndContract = Observable.CombineLatest(
                this.WhenAnyObservable(x => x.Router.CurrentViewModel),
                this.WhenAnyObservable(x => x.ViewContractObservable),
                (vm, contract) => Tuple.Create(vm, contract));

            // NB: The DistinctUntilChanged is useful because most views in 
            // WinRT will end up getting here twice - once for configuring
            // the RoutedViewHost's ViewModel, and once on load via SizeChanged
            vmAndContract.DistinctUntilChanged().Subscribe(x =>
            {

                this.IsBackTransition = AppVM != null && AppVM.NavigatingBack;
                if (x.Item1 == null || x.Item1 is IMainViewModel) // allows including the initial view directly to DefaultContent
                {
                    Content = DefaultContent;
                    if (AppVM != null)
                    {
                        AppVM.NavigatingBack = false;
                    }
                    return;
                }

                var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                var view = viewLocator.ResolveView(x.Item1, x.Item2) ?? viewLocator.ResolveView(x.Item1, null);

                if (view == null)
                {
                    throw new Exception(String.Format("Couldn't find view for '{0}'.", x.Item1));
                }
                view.ViewModel = x.Item1;
                Content = view;

                if (AppVM != null)
                {
                    AppVM.NavigatingBack = false;
                }

            }, ex => RxApp.DefaultExceptionHandler.OnNext(ex));
        }
    }



}

