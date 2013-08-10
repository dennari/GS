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




    public class AppViewModel : ReactiveObject, IScreen, IApplicationRootState
    {
        IRoutingState _Router;
        public IRoutingState Router
        {
            get { return _Router ?? (_Router = new RoutingState()); }
        }

        private ObservableAsPropertyHelper<IMyVM> _CurrentVM;
        public IMyVM CurrentVM { get { return _CurrentVM.Value; } }

        public AppViewModel()
        {


            var Resolver = RxApp.MutableResolver;
            Resolver.RegisterLazySingleton(() => new Page1(), typeof(IViewFor<Page1ViewModel>));
            Resolver.RegisterLazySingleton(() => new Page2(), typeof(IViewFor<Page2ViewModel>));
            Resolver.RegisterLazySingleton(() => new MessageBus(), typeof(IMessageBus));
            Resolver.RegisterLazySingleton(() => new Page1ViewModel(this,
                Resolver.GetService<IThreadIdFactory>(),
                Resolver.GetService<IMessageBus>()),
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
                .ToProperty(this, x => x.CurrentVM, out _CurrentVM);


            Router.NavigateCommandFor<Page1ViewModel>().Execute(null);
        }


    }
}
