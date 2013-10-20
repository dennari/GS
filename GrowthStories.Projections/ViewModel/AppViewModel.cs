
using ReactiveUI;
using System.Linq;

using System;
using System.Collections.Generic;
using Ninject;
using Growthstories.Domain.Entities;
using Growthstories.Domain;
using Growthstories.Core;
using Growthstories.Sync;
using Growthstories.Domain.Messaging;
using System.Threading.Tasks;
using Growthstories.UI.Services;
using CommonDomain;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{

    public interface IUserViewModel
    {

    }

    public interface IGSAppViewModel : IGSRoutableViewModel, IScreen, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
    {
        bool IsInDesignMode { get; }
        string AppName { get; }
        IMessageBus Bus { get; }
        IUserService Context { get; }
        IDictionary<IconType, Uri> IconUri { get; }
        IDictionary<IconType, Uri> BigIconUri { get; }
        IMutableDependencyResolver Resolver { get; }
        GSApp Model { get; }
        T SetIds<T>(T cmd, Guid? parentId = null, Guid? ancestorId = null) where T : IAggregateCommand;

        //Task<SyncResult> Synchronize();

        //IObservable<IUserViewModel> Users();
        //IObservable<IGardenViewModel> Gardens { get; }
        //IObservable<IPlantViewModel> Plants { get; }
        //IObservable<IPlantActionViewModel> PlantActions(Guid guid);


        //IPlantActionViewModel PlantActionViewModelFactory<T>(PlantActionState state = null) where T : IPlantActionViewModel;
        IObservable<IPlantActionViewModel> CurrentPlantActions(PlantState state, Guid? PlantActionId = null);
        IObservable<IPlantActionViewModel> FuturePlantActions(PlantState state, Guid? PlantActionId = null);

        IObservable<IPlantViewModel> CurrentPlants(IAuthUser user);
        IObservable<IPlantViewModel> FuturePlants(IAuthUser user);

        IObservable<IGardenViewModel> CurrentGardens(IAuthUser user = null);
        IObservable<IGardenViewModel> FutureGardens(IAuthUser user = null);


        ScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType);
        AddPlantViewModel AddPlantViewModelFactory(PlantState state);

        PageOrientation Orientation { get; }
        Task AddTestData();
        Task ClearDB();


        //IGardenViewModel GardenFactory(Guid guid);


    }


    public class AppViewModel : ReactiveObject, IGSAppViewModel
    {

        protected ObservableAsPropertyHelper<SupportedPageOrientation> _SupportedOrientations;
        public SupportedPageOrientation SupportedOrientations
        {
            get
            {
                return _SupportedOrientations.Value;
            }
        }

        public T SetIds<T>(T cmd, Guid? parentId = null, Guid? ancestorId = null)
            where T : IAggregateCommand
        {

            Guid AncestorId = ancestorId ?? this.Context.CurrentUser.Id;
            cmd.StreamAncestorId = AncestorId;
            cmd.AncestorId = AncestorId;
            if (parentId != null)
            {
                cmd.ParentId = parentId;
                cmd.ParentAncestorId = AncestorId;
            }
            return cmd;

        }

        public const string APPNAME = "GROWTH STORIES";

        public string AppName { get { return APPNAME; } }

        public IMessageBus Bus { get; protected set; }

        public bool IsInDesignMode
        {
            get
            {
                return DebugDesignSwitch ? true : DesignModeDetector.IsInDesignMode();
            }
        }

        IRoutingState _Router;
        public IRoutingState Router
        {
            get { return _Router ?? (_Router = new RoutingState()); }
        }

        public Guid GardenId { get; set; }


        private bool DebugDesignSwitch = false;

        public IMutableDependencyResolver Resolver { get; protected set; }

        protected IKernel Kernel;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public AppViewModel()
        {

            var resolver = new ModernDependencyResolver();
            resolver.InitializeResolver();
            RxApp.DependencyResolver = resolver;

            this.Resolver = resolver;

            resolver.RegisterConstant(this, typeof(IScreen));
            resolver.RegisterConstant(this.Router, typeof(IRoutingState));


            this.Router.CurrentViewModel.Subscribe(x =>
            {
                UpdateAppBar();
                UpdateMenuItems();
            });

            this.Router.CurrentViewModel
                .OfType<IHasAppBarButtons>()
                .Select(x => x.WhenAny(y => y.AppBarButtons, y => y.GetValue()).StartWith(x.AppBarButtons))
                .Switch()
                .Subscribe(x => UpdateAppBar(x));

            this.Router.CurrentViewModel
                .OfType<IHasMenuItems>()
                .Select(x => x.WhenAny(y => y.AppBarMenuItems, y => y.GetValue()).StartWith(x.AppBarMenuItems))
                .Switch()
                .Subscribe(x => UpdateMenuItems(x));

            this.Router.CurrentViewModel
                .OfType<IControlsAppBar>()
                .Select(x => x.WhenAny(y => y.AppBarMode, y => y.GetValue()).StartWith(x.AppBarMode))
                .Switch()
                .ToProperty(this, x => x.AppBarMode, out this._AppBarMode, ApplicationBarMode.MINIMIZED);

            this.Router.CurrentViewModel
                 .OfType<IControlsAppBar>()
                 .Select(x => x.WhenAny(y => y.AppBarIsVisible, y => y.GetValue()).StartWith(x.AppBarIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.AppBarIsVisible, out this._AppBarIsVisible, true);

            this.Router.CurrentViewModel
                 .OfType<IControlsSystemTray>()
                 .Select(x => x.WhenAny(y => y.SystemTrayIsVisible, y => y.GetValue()).StartWith(x.SystemTrayIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.SystemTrayIsVisible, out this._SystemTrayIsVisible, false);

            this.Router.CurrentViewModel
                 .OfType<IControlsProgressIndicator>()
                 .Select(x => x.WhenAny(y => y.ProgressIndicatorIsVisible, y => y.GetValue()).StartWith(x.ProgressIndicatorIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.ProgressIndicatorIsVisible, out this._ProgressIndicatorIsVisible, true);



            this.Router.CurrentViewModel
                //.OfType<IControlsPageOrientation>()
                .Select(x =>
                {
                    var xx = x as IControlsPageOrientation;
                    if (xx != null)
                        return xx.WhenAny(y => y.SupportedOrientations, y => y.GetValue()).StartWith(xx.SupportedOrientations);
                    return Observable.Return(SupportedPageOrientation.Portrait);
                })
                .Switch()
                .ToProperty(this, x => x.SupportedOrientations, out this._SupportedOrientations, SupportedPageOrientation.Portrait);

            resolver.RegisterLazySingleton(() => new MainViewModel(this, new GardenViewModel(this.Context.CurrentUser, this)), typeof(IMainViewModel));
            resolver.RegisterLazySingleton(() => new NotificationsViewModel(this), typeof(INotificationsViewModel));

            resolver.RegisterLazySingleton(() => new FriendsViewModel(this), typeof(FriendsViewModel));


            resolver.RegisterLazySingleton(() => new ListUsersViewModel(
                Kernel.Get<ITransportEvents>(),
                this), typeof(ListUsersViewModel));

            //resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));



        }


        protected GSApp _Model;
        public GSApp Model
        {
            get
            {

                return _Model;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _Model, value);
            }
        }



        protected IDispatchCommands _Handler;
        protected IDispatchCommands Handler
        {
            get
            {
                return _Handler ?? (_Handler = Kernel.Get<IDispatchCommands>());
            }
        }

        protected GSApp Initialize(IGSRepository repository)
        {


            // Subscriptions where we need to catch all the happenings
            Bus.Listen<IAggregateCommand>().Subscribe(x =>
            {
                Handler.Handle(x);
            });



            GSApp app = null;

            try
            {
                app = (GSApp)repository.GetById(GSAppState.GSAppId);
            }
            catch (DomainError e)
            {

            }

            if (app == null)
            {
                app = (GSApp)Handler.Handle(new CreateGSApp());
                //app = Bus.ListenIncludeLatest<IEvent>()
                //        .OfType<GSAppCreated>()
                //        .Select(x => (GSApp)repository.GetById(x.AggregateId))
                //        .Take(1)
                //        .GetAwaiter()
                //        .Wait();
                //app = factory.Build<GSApp>();

            }

            this.Model = app;

            ((AppUserService)Context).SetupCurrentUser(this);

            return app;
        }

        IUserService _Context = null;
        public IUserService Context
        {
            get
            {
                if (_Context == null)
                {
                    var s = Kernel.Get<AppUserService>();
                    _Context = s;

                }
                return _Context;
            }
        }

        //protected IGSRepository Repository;
        //protected IObservable<T> GetById<T>(Guid id) where T : IGSAggregate
        //{
        //    if (Repository == null)
        //        Repository = Kernel.Get<IGSRepository>();
        //    //Func<T> f = () => (T)Repository.GetById(id);
        //    var subj = new AsyncSubject<T>();
        //    var obs = Observable.Start(() => (T)Repository.GetById(id), RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler)
        //        .Subscribe(x =>
        //        {
        //            subj.OnNext(x);
        //            subj.OnCompleted();
        //        });

        //    return subj;

        //    //return //f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

        //}

        //public IGardenViewModel GardenFactory(UserState uState)
        //{

        //    //var userObs = GetById<User>(userId);

        //    //var gardenObs = userObs.Select(x => Tuple.Create(x.State, x.State.Gardens[x.State.GardenId]));

        //    var plantObs1 = Bus.Listen<IEvent>().OfType<PlantCreated>().Where(x => x.UserId == uState.Id)
        //                        .Select(x =>
        //                        {
        //                            return new PlantViewModel(x.State, this);
        //                        });


        //    var plantObs2 = gardenObs.Select(x => x.Item2)
        //                    .Select(x => x.PlantIds.ToObservable())
        //                    .Merge()
        //                    .Select(y => GetById<Plant>(y))
        //                    .Merge()
        //                    .Select(x => new PlantViewModel(x.State, this));

        //    var plantObs = Observable.Merge(plantObs1, plantObs2);

        //    return new GardenViewModel(
        //        gardenObs,
        //        plantObs,
        //        this
        //    );

        //}

        IObservable<IGardenViewModel> _Gardens;
        public IObservable<IGardenViewModel> FutureGardens(IAuthUser user = null)
        {


            if (_Gardens == null)
            {
                var U = Bus.Listen<IEvent>()
                    .OfType<UserCreated>();

                var garden = Bus.Listen<IEvent>()
                    .OfType<GardenAdded>();



                _Gardens = Observable.CombineLatest(U, garden, (u, g) => Tuple.Create(u, g))
                    .Where(x =>
                    {
                        return x.Item1.AggregateId == x.Item2.AggregateId;
                    })
                    .DistinctUntilChanged()
                    .Select(x => new GardenViewModel(x.Item2.AggregateState, this));
            }

            if (user != null)
                return _Gardens.Where(x => x.UserState.Id == user.Id);
            return _Gardens;

        }


        public IObservable<IGardenViewModel> CurrentGardens(IAuthUser user = null)
        {


            Func<Guid?, IEnumerable<UserState>> f = UIPersistence.GetUsers;

            var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            Guid? id = null;
            if (user != null)
                id = user.Id;

            var current = af(id)
                //.OfType<User>()
                .Select(x => x.ToObservable())
                .Switch()
                .Select(x => new GardenViewModel(x, this));

            return current;

        }



        protected IUIPersistence _UIPersistence;
        protected IUIPersistence UIPersistence { get { return _UIPersistence ?? (_UIPersistence = Kernel.Get<IUIPersistence>()); } }



        public IObservable<IPlantActionViewModel> CurrentPlantActions(PlantState state, Guid? PlantActionId = null)
        {


            Func<Guid?, Guid?, Guid?, IEnumerable<PlantActionState>> f = UIPersistence.GetActions;

            var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            var current = af(PlantActionId, state.Id, null)
                //.OfType<User>()
                .Select(x => x.ToObservable())
                .Switch()
                .Select(x => PlantActionViewModelFactory<IPlantActionViewModel>(x));

            return current;

        }

        public IObservable<IPlantActionViewModel> FuturePlantActions(PlantState state, Guid? PlantActionId = null)
        {

            return Bus.Listen<IEvent>()
                    .OfType<PlantActionCreated>()
                    .Where(x => x.PlantId == state.Id)
                    .Select(x => PlantActionViewModelFactory<IPlantActionViewModel>(x.AggregateState));
        }





        public IObservable<IPlantViewModel> CurrentPlants(IAuthUser user)
        {


            Func<Guid?, Guid?, Guid?, IEnumerable<PlantState>> f = UIPersistence.GetPlants;

            var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            var current = af(null, null, user.Id)
                //.OfType<User>()
                .Select(x => x.ToObservable())
                .Switch()
                .Select(x => new PlantViewModel(x, this));

            return current;

        }

        public IObservable<IPlantViewModel> FuturePlants(IAuthUser user)
        {
            return Bus.Listen<IEvent>()
               .OfType<PlantCreated>()
               .Where(x =>
               {
                   return x.UserId == user.Id;
               })
               .Select(x => new PlantViewModel(x.AggregateState, this));
        }




        public virtual IPlantActionViewModel PlantActionViewModelFactory<T>(PlantActionState state = null) where T : IPlantActionViewModel
        {

            if (state != null)
            {
                if (state.Type == PlantActionType.COMMENTED)
                    return new PlantCommentViewModel(state, this);
                if (state.Type == PlantActionType.FERTILIZED)
                    return new PlantFertilizeViewModel(state, this);
                if (state.Type == PlantActionType.WATERED)
                    return new PlantWaterViewModel(state, this);
                if (state.Type == PlantActionType.MEASURED)
                    return new PlantMeasureViewModel(state, this);
                if (state.Type == PlantActionType.PHOTOGRAPHED)
                    return new PlantPhotographViewModel(state, this);
            }

            var t = typeof(T);
            if (t == typeof(IPlantCommentViewModel))
                return new PlantCommentViewModel(state, this);
            if (t == typeof(IPlantFertilizeViewModel))
                return new PlantFertilizeViewModel(state, this);
            if (t == typeof(IPlantWaterViewModel))
                return new PlantWaterViewModel(state, this);
            if (t == typeof(IPlantMeasureViewModel))
                return new PlantMeasureViewModel(state, this);
            if (t == typeof(IPlantPhotographViewModel))
                return new PlantPhotographViewModel(state, this);

            return null;
        }



        //public IPlantViewModel PlantFactory(Guid id)
        //{
        //    return new PlantViewModel(
        //     ((Plant)Kernel.Get<IGSRepository>().GetById(id)).State,
        //     this
        //    );

        //}

        protected ObservableAsPropertyHelper<bool> _AppBarIsVisible;
        public bool AppBarIsVisible
        {
            get { return _AppBarIsVisible.Value; }
        }

        protected ObservableAsPropertyHelper<bool> _SystemTrayIsVisible;
        public bool SystemTrayIsVisible
        {
            get { return _SystemTrayIsVisible.Value; }
        }

        protected ObservableAsPropertyHelper<bool> _ProgressIndicatorIsVisible;
        public bool ProgressIndicatorIsVisible
        {
            get { return _ProgressIndicatorIsVisible.Value; }
        }

        protected ObservableAsPropertyHelper<ApplicationBarMode> _AppBarMode;
        public ApplicationBarMode AppBarMode
        {
            get { return _AppBarMode.Value; }
        }

        private void UpdateAppBar(IList<ButtonViewModel> x = null)
        {
            this.AppBarButtons.RemoveRange(0, this.AppBarButtons.Count);
            if (x != null)
                this.AppBarButtons.AddRange(x);
        }

        private void UpdateMenuItems(IList<MenuItemViewModel> x = null)
        {
            this.AppBarMenuItems.RemoveRange(0, this.AppBarMenuItems.Count);
            if (x != null)
                this.AppBarMenuItems.AddRange(x);
        }


        protected ReactiveList<ButtonViewModel> _AppBarButtons = new ReactiveList<ButtonViewModel>();
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
        }

        protected ReactiveList<MenuItemViewModel> _AppBarMenuItems = new ReactiveList<MenuItemViewModel>();
        public ReactiveList<MenuItemViewModel> AppBarMenuItems
        {
            get { return _AppBarMenuItems; }
        }

        IDictionary<IconType, Uri> _IconUri = new Dictionary<IconType, Uri>()
        {
            {IconType.ADD,new Uri("/Assets/Icons/appbar.add.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK,new Uri("/Assets/Icons/appbar.check.png", UriKind.RelativeOrAbsolute)},
            {IconType.DELETE,new Uri("/Assets/Icons/appbar.delete.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK_LIST,new Uri("/Assets/Icons/appbar.list.check.png", UriKind.RelativeOrAbsolute)},
            {IconType.WATER,new Uri("/Assets/Icons/icon_watering_appbar.png", UriKind.RelativeOrAbsolute)},
            {IconType.PHOTO,new Uri("/Assets/Icons/icon_photo_appbar.png", UriKind.RelativeOrAbsolute)},
            {IconType.FERTILIZE,new Uri("/Assets/Icons/icon_nutrient_appbar.png", UriKind.RelativeOrAbsolute)},
            {IconType.NOTE,new Uri("/Assets/Icons/icon_comment_appbar.png", UriKind.RelativeOrAbsolute)},
            {IconType.MEASURE,new Uri("/Assets/Icons/icon_length_appbar.png", UriKind.RelativeOrAbsolute)},
            {IconType.SHARE,new Uri("/Assets/Icons/appbar.social.sharethis.png", UriKind.RelativeOrAbsolute)}

        };

        public IDictionary<IconType, Uri> IconUri { get { return _IconUri; } }



        IDictionary<IconType, Uri> _bIconUri = new Dictionary<IconType, Uri>()
        {
            {IconType.WATER,new Uri("/Assets/Icons/icon_watering.png", UriKind.RelativeOrAbsolute)},
            {IconType.PHOTO,new Uri("/Assets/Icons/icon_photo.png", UriKind.RelativeOrAbsolute)},
            {IconType.FERTILIZE,new Uri("/Assets/Icons/icon_nutrient.png", UriKind.RelativeOrAbsolute)},
            {IconType.NOTE,new Uri("/Assets/Icons/icon_comment.png", UriKind.RelativeOrAbsolute)},
            {IconType.MEASURE,new Uri("/Assets/Icons/icon_length.png", UriKind.RelativeOrAbsolute)}
        };

        public IDictionary<IconType, Uri> BigIconUri { get { return _bIconUri; } }






        //ISynchronizerService _SyncService;
        //public Task<SyncResult> Synchronize()
        //{
        //    if (_SyncService == null)
        //        _SyncService = Kernel.Get<ISynchronizerService>();



        //    //var syncStreams = ;
        //    return _SyncService.Synchronize(Model);
        //}

        public PageOrientation _Orientation;
        public PageOrientation Orientation
        {
            get { return _Orientation; }
            set { this.RaiseAndSetIfChanged(ref _Orientation, value); }
        }

        private ReactiveCommand _PageOrientationChangedCommand;
        public ReactiveCommand PageOrientationChangedCommand
        {
            get
            {

                if (_PageOrientationChangedCommand == null)
                {
                    _PageOrientationChangedCommand = new ReactiveCommand();
                    _PageOrientationChangedCommand.Subscribe(x =>
                    {
                        try
                        {
                            this.Orientation = (PageOrientation)x;

                        }
                        catch
                        {

                        }
                    });
                }
                return _PageOrientationChangedCommand;

            }
        }


        public IScreen HostScreen
        {
            get { return this; }
        }

        public ScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType)
        {
            ScheduleState state = null;
            if (plantState != null)
            {
                Guid id = scheduleType == ScheduleType.FERTILIZING ? plantState.FertilizingScheduleId : plantState.WateringScheduleId;
                //if (id != default(Guid))
                //    state = ((Schedule)Kernel.Get<IGSRepository>().GetById(id)).State;
            }
            return new ScheduleViewModel(state, scheduleType, this);
        }


        public string PageTitle
        {
            get { throw new NotImplementedException(); }
        }

        public string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public virtual AddPlantViewModel AddPlantViewModelFactory(PlantState state)
        {
            throw new NotImplementedException();
        }

        public virtual Task AddTestData()
        {

            throw new NotImplementedException();

        }

        public virtual Task ClearDB()
        {
            throw new NotImplementedException();

        }





    }

    public enum View
    {
        EXCEPTION,
        GARDEN,
        PLANT,
        ADD_PLANT,
        ADD_COMMENT,
        ADD_WATER,
        ADD_PHOTO,
        ADD_FERT,
        SELECT_PROFILE_PICTURE
    }

    public enum IconType
    {
        ADD,
        CHECK,
        CANCEL,
        DELETE,
        CHECK_LIST,
        WATER,
        FERTILIZE,
        PHOTO,
        NOTE,
        MEASURE,
        NOURISH,
        CHANGESOIL,
        SHARE
    }

    public enum ApplicationBarMode
    {
        DEFAULT,
        MINIMIZED
    }



}

