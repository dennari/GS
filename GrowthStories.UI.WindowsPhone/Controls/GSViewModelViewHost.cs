using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
//using ReactiveUI.Xaml;

using System.Windows.Controls;
using ReactiveUI;


namespace Growthstories.UI.WindowsPhone
{
    /// <summary>
    /// This content control will automatically load the View associated with
    /// the ViewModel property and display it. This control is very useful
    /// inside a DataTemplate to display the View associated with a ViewModel.
    /// </summary>
    public class GSViewModelViewHost : ContentControl
    {
        /// <summary>
        /// The ViewModel to display
        /// </summary>
        public object ViewModel
        {
            get { return GetValue(ViewModelProperty); }
            set
            {
                if (ViewModel != value)
                    SetValue(ViewModelProperty, value);
            }
        }

        //public static readonly DependencyProperty ViewModelProperty = 
        //   DependencyProperty.Register("ViewModel", typeof(object), typeof(GSViewModelViewHost), new PropertyMetadata(null, somethingChanged));

        public static readonly DependencyProperty ViewModelProperty =
           DependencyProperty.Register("ViewModel", typeof(object), typeof(GSViewModelViewHost), new PropertyMetadata(null, ViewModelChanged));


        /// <summary>
        /// If no ViewModel is displayed, this content (i.e. a control) will be displayed.
        /// </summary>
        public object DefaultContent
        {
            get { return GetValue(DefaultContentProperty); }
            set
            {
                if (DefaultContent != value)
                    SetValue(DefaultContentProperty, value);
            }
        }
        public static readonly DependencyProperty DefaultContentProperty =
            DependencyProperty.Register("DefaultContent", typeof(object), typeof(GSViewModelViewHost), new PropertyMetadata(null, DefaultContentChanged));

        public IObservable<string> ViewContractObservable
        {
            get { return (IObservable<string>)GetValue(ViewContractObservableProperty); }
            set { SetValue(ViewContractObservableProperty, value); }
        }
        public static readonly DependencyProperty ViewContractObservableProperty =
            DependencyProperty.Register("ViewContractObservable", typeof(IObservable<string>), typeof(GSViewModelViewHost), new PropertyMetadata(Observable.Return(default(string))));

        public IViewLocator ViewLocator { get; set; }

        public GSViewModelViewHost()
        {
            //var vmAndContract = Observable.CombineLatest(
            //    this.WhenAnyValue(x => x.ViewModel),
            //    this.WhenAnyObservable(x => x.ViewContractObservable),
            //    (vm, contract) => new { ViewModel = vm, Contract = contract, });

            //var platform = RxApp.DependencyResolver.GetService<IPlatformOperations>();
            //if (platform == null) {
            //    throw new Exception("Couldn't find an IPlatformOperations. This should never happen, your dependency resolver is broken");
            //}

            //ViewContractObservable = Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(x => SizeChanged += x, x => SizeChanged -= x)
            //    .Select(_ => platform.GetOrientation())
            //    .DistinctUntilChanged()
            //    .StartWith(platform.GetOrientation())
            //    .Select(x => x != null ? x.ToString() : default(string));

            this.Content = this.DefaultContent;
        }

        protected void OnViewModelChanged(object x)
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
        }


        public static void DefaultContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var me = sender as GSViewModelViewHost;
            var v = e.NewValue;
            if (me != null)
            {
                me.DefaultContent = v;
                if (me.ViewModel == null)
                    me.Content = v;
            }
        }




        public static void ViewModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var me = sender as GSViewModelViewHost;
            var v = e.NewValue;
            if (me != null)
            {
                me.ViewModel = v;
                me.OnViewModelChanged(v);
                //if (me.ViewModel == null)
                //    me.Content = v;
            }
        }

    }
}
