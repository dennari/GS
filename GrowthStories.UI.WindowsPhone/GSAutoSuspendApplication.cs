using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Ninject;
using ReactiveUI;
using ReactiveUI.Mobile;
using System.Threading;
using HockeyApp;
using System.IO.IsolatedStorage;

namespace Growthstories.UI.WindowsPhone
{

    public class GSAutoSuspendApplication : Application, ISuspensionHost, IEnableLogger
    {

        public IObservable<Unit> IsLaunchingNew { get { return SuspensionHost.IsLaunchingNew; } }
        public IObservable<Unit> IsResuming { get { return SuspensionHost.IsResuming; } }
        public IObservable<Unit> IsUnpausing { get { return SuspensionHost.IsUnpausing; } }
        public IObservable<IDisposable> ShouldPersistState { get { return SuspensionHost.ShouldPersistState; } }
        public IObservable<Unit> ShouldInvalidateState { get { return SuspensionHost.ShouldInvalidateState; } }


        public static SuspensionHost SuspensionHost;

        readonly Subject<IApplicationRootState> _viewModelChanged = new Subject<IApplicationRootState>();
        IApplicationRootState _ViewModel;

        public IApplicationRootState ViewModel
        {
            get { return _ViewModel; }
            set
            {
                if (_ViewModel == value) return;
                _ViewModel = value;
                _viewModelChanged.OnNext(value);
            }
        }

        public static PhoneApplicationFrame RootFrame { get; protected set; }

        public static readonly Stopwatch LifeTimer = new Stopwatch();
        protected IKernel Kernel;
        protected PhoneApplicationService LifeTimeHelper { get; private set; }

        protected GSAutoSuspendApplication()
        {

            //"fe617508-961a-4981-a3c6-71e35e48b703"

            LifeTimer.Start();


            //this.Log().Info("Kernel creation {0}", LifeTimer.ElapsedMilliseconds);


            LifeTimeHelper = new PhoneApplicationService();
            ApplicationLifetimeObjects.Add(LifeTimeHelper);

            var host = new SuspensionHost();


            host.IsLaunchingNew =
                Observable.FromEventPattern<LaunchingEventArgs>(
                    x => LifeTimeHelper.Launching += x, x => LifeTimeHelper.Launching -= x)
                    .Select(_ => Unit.Default);

            host.IsUnpausing =
                Observable.FromEventPattern<ActivatedEventArgs>(
                    x => LifeTimeHelper.Activated += x, x => LifeTimeHelper.Activated -= x)
                    .Where(x => x.EventArgs.IsApplicationInstancePreserved)
                    .Select(_ => Unit.Default);

            // NB: "Applications should not perform resource-intensive tasks 
            // such as loading from isolated storage or a network resource 
            // during the Activated event handler because it increase the time 
            // it takes for the application to resume"
            host.IsResuming =
                Observable.FromEventPattern<ActivatedEventArgs>(
                    x => LifeTimeHelper.Activated += x, x => LifeTimeHelper.Activated -= x)
                    .Where(x => !x.EventArgs.IsApplicationInstancePreserved)
                    .Select(_ => Unit.Default)
                    .ObserveOn(RxApp.TaskpoolScheduler);

            // NB: No way to tell OS that we need time to suspend, we have to
            // do it in-process
            host.ShouldPersistState = Observable.Merge(
                Observable.FromEventPattern<DeactivatedEventArgs>(
                    x => LifeTimeHelper.Deactivated += x, x => LifeTimeHelper.Deactivated -= x)
                    .Select(_ => Disposable.Empty),
                Observable.FromEventPattern<ClosingEventArgs>(
                    x => LifeTimeHelper.Closing += x, x => LifeTimeHelper.Closing -= x)
                    .Select(_ => Disposable.Empty));

            host.ShouldInvalidateState =
                Observable.FromEventPattern<ApplicationUnhandledExceptionEventArgs>(x => UnhandledException += x, x => UnhandledException -= x)
                    .Select(_ => Unit.Default);

            SuspensionHost = host;

            SuspensionHost.IsLaunchingNew.Subscribe(_ =>
            {
                //ViewModel = RxApp.DependencyResolver.GetService<IApplicationRootState>();

                //this.Log().Info("IsLaunchingNew: setting ViewModel to an instance of {0}", ViewModel.GetType().Name);
            });

            //
            // Do the equivalent steps that the boilerplate code does for WP8 apps
            //

            if (RootFrame != null) return;
            RootFrame = new PhoneApplicationFrame();

            var currentBackHook = default(IDisposable);
            var currentViewFor = default(WeakReference<IViewFor>);


            RootFrame.Navigated += (o, e) =>
            {


                // Always clear the WP8 Back Stack, we're using our own
                this.Log().Info("RootFrame navigated begins {0}", LifeTimer.ElapsedMilliseconds);
                //this.Log().Info("GSAutoSuspendApplication constructor init {0}", LifeTimer.ElapsedMilliseconds);


                while (RootFrame.RemoveBackEntry() != null) { }

                if (currentBackHook != null) currentBackHook.Dispose();
                var page = RootFrame.Content as PhoneApplicationPage;
                if (page != null)
                {
                    currentBackHook = Observable.FromEventPattern<CancelEventArgs>(x => page.BackKeyPress += x, x => page.BackKeyPress -= x)
                        .Where(x => ViewModel != null && !x.EventArgs.Cancel)
                        .Subscribe(x =>
                        {
                            if (!ViewModel.Router.NavigateBack.CanExecute(null)) return;

                            x.EventArgs.Cancel = true;
                            ViewModel.Router.NavigateBack.Execute(null);
                        });

                    var viewFor = page as IViewFor;
                    if (viewFor == null)
                    {
                        throw new Exception("Your Main Page (i.e. the one that is pointed to by WMAppManifest) must implement IViewFor<YourAppBootstrapperClass>");
                    }
                    //this.Log().Info("Setting ViewModel to an instance of {0} for View {1}", ViewModel.GetType().Name, viewFor.GetType().Name);

                    currentViewFor = new WeakReference<IViewFor>(viewFor);
                    //viewFor.ViewModel = ViewModel;
                }
                this.Log().Info("RootFrame navigated ends {0}", LifeTimer.ElapsedMilliseconds);

                // Finally make it live
                RootVisual = RootFrame;
            };

            _viewModelChanged.StartWith(ViewModel).Where(x => x != null).ObserveOn(RxApp.MainThreadScheduler).Subscribe(vm =>
            {
                var viewFor = default(IViewFor);

                if (currentViewFor != null && currentViewFor.TryGetTarget(out viewFor))
                {
                    viewFor.ViewModel = vm;
                    this.Log().Info("Setting ViewModel to an instance of {0} for View {1}", ViewModel.GetType().Name, viewFor.GetType().Name);

                }
            });


            // SET UP HOCKEYAPP
            HockeyApp.CrashHandler.Instance.Configure(this, "edaad6a078ea7a024e0c50661ac8a64a", RootFrame);

            // SET UP CUSTOM SYNC CONTEXT
            //var sContext = AsyncSynchronizationContext.Register();
            //sContext.Handler = e =>
            //{
            //    if (Debugger.IsAttached) Debugger.Break();
            //    //e.Handled = true;
            //    RxApp.MainThreadScheduler.Schedule(() =>
            //    {
            //        throw e;
            //    });
            //    if (e != null)
            //        Bootstrap.HandleUnhandledExceptions(e, this);

            //};

            UnhandledException += (o, e) =>
            {
                if (Debugger.IsAttached) Debugger.Break();
                Bootstrap.PossiblyMarkExceptionHandled(e);

                if (e.ExceptionObject != null)
                {
                    Bootstrap.HandleUnhandledExceptions(e.ExceptionObject, this);
                }
                else
                {
                    Bootstrap.HandleUnhandledExceptions(
                        new Exception("exceptionobject was null for unhandledexception"), this);
                }
            };

            RootFrame.NavigationFailed += (o, e) =>
            {
                if (Debugger.IsAttached) Debugger.Break();

                Bootstrap.PossiblyMarkExceptionHandled(e);
                if (e.Exception != null)
                {
                    Bootstrap.HandleUnhandledExceptions(e.Exception, this);
                }
                else
                {
                    Bootstrap.HandleUnhandledExceptions(
                        new Exception("exceptionobject was null for unhandledexception"), this);
                }
            };

            TaskScheduler.UnobservedTaskException += (o, e) =>
            {
                if (Debugger.IsAttached) Debugger.Break();

                Bootstrap.PossiblyMarkExceptionHandled(e);
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    if (e.Exception != null)
                    {
                        throw e.Exception;
                    }
                    else
                    {
                        throw new Exception("exceptionobject was null for unobservedtaskexception");
                    }
                });
            };

            Task.Run(() =>
            {
                //var xamlElapsed = stopwatch.Elapsed;
                //stopwatch.Restart();
                //var kernelElapsed = stopwatch.Elapsed;
                //stopwatch.Restart();
                this.Kernel = new StandardKernel(Bootstrap.GetModule(this));
                this.ViewModel = Kernel.Get<IApplicationRootState>();

                this.Log().Info("Loaded?");
                //var appVmElapsed = stopwatch.Elapsed;
                //stopwatch.Stop();
            });
        }


        public void SetupDefaultSuspendResume(ISuspensionDriver driver = null)
        {
            //driver = driver ?? RxApp.DependencyResolver.GetService<ISuspensionDriver>();

            //SuspensionHost.ShouldInvalidateState
            //    .SelectMany(_ => driver.InvalidateState())
            //    .LoggedCatch(this, Observable.Return(Unit.Default), "Tried to invalidate app state")
            //    .Subscribe(_ => this.Log().Info("Invalidated app state"));

            //SuspensionHost.ShouldPersistState
            //    .SelectMany(x => driver.SaveState(ViewModel).Finally(x.Dispose))
            //    .LoggedCatch(this, Observable.Return(Unit.Default), "Tried to persist app state")
            //    .Subscribe(_ => this.Log().Info("Persisted application state"));

            //SuspensionHost.IsResuming
            //    .SelectMany(x => driver.LoadState<IApplicationRootState>())
            //    .LoggedCatch(this,
            //        Observable.Defer(() => Observable.Return(RxApp.DependencyResolver.GetService<IApplicationRootState>())),
            //        "Failed to restore app state from storage, creating from scratch")
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(x => ViewModel = x);

            //SuspensionHost.IsLaunchingNew.Subscribe(_ =>
            //{
            //    ViewModel = RxApp.DependencyResolver.GetService<IApplicationRootState>();
            //});
        }

    }

    public class AsyncSynchronizationContext : SynchronizationContext
    {

        public Action<Exception> Handler = null;

        public static AsyncSynchronizationContext Register()
        {
            var syncContext = Current;
            if (syncContext == null)
                throw new InvalidOperationException("Ensure a synchronization context exists before calling this method.");

            var customSynchronizationContext = syncContext as AsyncSynchronizationContext;

            if (customSynchronizationContext == null)
            {
                customSynchronizationContext = new AsyncSynchronizationContext(syncContext);
                SetSynchronizationContext(customSynchronizationContext);
            }

            return customSynchronizationContext;
        }

        private readonly SynchronizationContext _syncContext;

        public AsyncSynchronizationContext(SynchronizationContext syncContext)
        {
            _syncContext = syncContext;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new AsyncSynchronizationContext(_syncContext.CreateCopy());
        }

        public override void OperationCompleted()
        {
            _syncContext.OperationCompleted();
        }

        public override void OperationStarted()
        {
            _syncContext.OperationStarted();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _syncContext.Post(WrapCallback(d), state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _syncContext.Send(d, state);
        }

        private SendOrPostCallback WrapCallback(SendOrPostCallback sendOrPostCallback)
        {
            return state =>
            {
                Exception exception = null;

                try
                {
                    sendOrPostCallback(state);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                if (exception != null && Handler != null)
                    Handler(exception);

            };
        }
    }
}
