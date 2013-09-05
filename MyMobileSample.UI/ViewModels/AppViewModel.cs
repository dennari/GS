using ReactiveUI;
using ReactiveUI.Mobile;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;
using MyMobileSample.UI.Views;
using MyMobileSample.Model.ViewModels;
using System.Collections.Generic;
using System;
using MyMobileSample.Model;
using System.ComponentModel;

namespace MyMobileSample.UI.ViewModels
{
    public class ThreadIdFactory : IThreadIdFactory
    {

        public int CurrentId
        {
            get
            {
                return System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
        }
    }


    public static class AppExtensions
    {
        private static bool? _isInDesignMode;

        public static bool IsInDesignMode()
        {

            if (!_isInDesignMode.HasValue)
            {
#if SILVERLIGHT
                _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
#if NETFX_CORE
                    _isInDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                                     .FromProperty(prop, typeof(FrameworkElement))
                                     .Metadata.DefaultValue;
#endif
#endif
            }

            return _isInDesignMode.Value;
        }
    }

    public class AppViewModel : ReactiveObject, IScreen, IApplicationRootState
    {
        IRoutingState _Router;
        public IRoutingState Router
        {
            get { return _Router ?? (_Router = new RoutingState()); }
        }

        private IMyVM _CurrentVM;
        public IMyVM CurrentVM { get { return _CurrentVM; } set { this.RaiseAndSetIfChanged(ref _CurrentVM, value); } }

        public Page1ViewModel DesignViewModel1 { get; private set; }
        public Page2ViewModel DesignViewModel2 { get; private set; }


        public AppViewModel()
        {


            var Resolver = RxApp.MutableResolver;
            Resolver.RegisterLazySingleton(() => new Page1(), typeof(IViewFor<Page1ViewModel>));
            Resolver.RegisterLazySingleton(() => new Page2(), typeof(IViewFor<Page2ViewModel>));
            Resolver.RegisterLazySingleton(() => new MessageBus(), typeof(IMessageBus));
            Resolver.RegisterLazySingleton(() => new Page1ViewModel(this,
                Resolver.GetService<IThreadIdFactory>(),
                Resolver.GetService<IMessageBus>(), AppExtensions.IsInDesignMode()),
                typeof(Page1ViewModel)
            );
            Resolver.RegisterLazySingleton(() => new Page2ViewModel(this, Resolver.GetService<IThreadIdFactory>(), Resolver.GetService<IMessageBus>()), typeof(Page2ViewModel));
            Resolver.RegisterConstant(this, typeof(IApplicationRootState));
            Resolver.RegisterConstant(this, typeof(IScreen));
            Resolver.RegisterConstant(Router, typeof(IRoutingState));
            Resolver.RegisterLazySingleton(() => new ThreadIdFactory(), typeof(IThreadIdFactory));

            Router
                .NavigationStack
                .ItemsAdded
                .OfType<IMyVM>()
                .Subscribe(x => x.SetupAppBarButtons());
            Router
                .NavigationStack
                .BeforeItemsAdded
                .OfType<IMyVM>()
                .Where(x => CurrentVM != null)
                .Subscribe(x => CurrentVM.ClearAppBarButtons());
            Router
                .CurrentViewModel
                .OfType<IMyVM>()
                .Subscribe(x => this.CurrentVM = x);


            Router.NavigateCommandFor<Page1ViewModel>().Execute(null);

            if (AppExtensions.IsInDesignMode())
            {
                this.DesignViewModel1 = Resolver.GetService<Page1ViewModel>();
                this.DesignViewModel2 = Resolver.GetService<Page2ViewModel>();


            }
        }


    }
}
