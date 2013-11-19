
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
using System.Reactive.Disposables;

namespace Growthstories.UI.ViewModel
{




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

            if (this.Context == null || this.Context.CurrentUser == null)
                return cmd;

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

        protected IDispatchCommands _Handler;
        protected IDispatchCommands Handler
        {
            get { return _Handler ?? (_Handler = Kernel.Get<IDispatchCommands>()); }
        }

        protected IGSRepository _Repository;
        protected IGSRepository Repository
        {
            get { return _Repository ?? (_Repository = Kernel.Get<IGSRepository>()); }
        }

        protected IUIPersistence _UIPersistence;
        protected IUIPersistence UIPersistence
        {
            get { return _UIPersistence ?? (_UIPersistence = Kernel.Get<IUIPersistence>()); }
        }


        IUserService _Context = null;
        public IUserService Context
        {
            get { return _Context ?? (_Context = Kernel.Get<IUserService>()); }
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
            resolver.RegisterLazySingleton(() => new GSViewLocator(), typeof(IViewLocator));


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

            resolver.RegisterLazySingleton(() => new GardenViewModel(null, this), typeof(IGardenViewModel));
            resolver.RegisterLazySingleton(() => new NotificationsViewModel(this), typeof(INotificationsViewModel));

            resolver.RegisterLazySingleton(() => new FriendsViewModel(this), typeof(FriendsViewModel));


            resolver.RegisterLazySingleton(() => new SearchUsersViewModel(
                Kernel.Get<ITransportEvents>(),
                this), typeof(SearchUsersViewModel));

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

        protected IAuthUser _User;
        public IAuthUser User
        {
            get
            {

                return _User;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _User, value);
            }
        }


        //Task<T> RunTask<T>(Func<T> f)
        //{
        //    var t = Task.Run(f);
        //    //t.ConfigureAwait(false);
        //    return t;
        //}

        Task<IGSAggregate> CurrentHandleJob;
        public Task<IGSAggregate> HandleCommand(IAggregateCommand x)
        {




            var prevJob = CurrentHandleJob;

            CurrentHandleJob = Task.Run(async () =>
            {
                if (prevJob != null && !prevJob.IsCompleted)
                {
                    await prevJob;
                }
                if (!x.AncestorId.HasValue)
                    this.SetIds(x);
                var push = x as Push;
                if (push != null)
                    return Handler.Handle(push);
                var pull = x as Pull;
                if (pull != null)
                    return Handler.Handle(pull);
                return Handler.Handle(x);
            });
            //CurrentHandleJob.ConfigureAwait(false);
            return CurrentHandleJob;
        }

        public Task<IGSAggregate> HandleCommand(MultiCommand x)
        {

            var prevJob = CurrentHandleJob;

            CurrentHandleJob = Task.Run(async () =>
            {
                if (prevJob != null && !prevJob.IsCompleted)
                {
                    await prevJob;
                }
                return Handler.Handle(x);
            });

            return CurrentHandleJob;
        }

        Task<IGSAggregate> CurrentGetByIdJob; // only for starting
        public Task<IGSAggregate> GetById(Guid id)
        {
            //CurrentGetByIdJob = CurrentGetByIdJob.ContinueWith(prev => RunTask(() => Repository.GetById(id))).Unwrap();
            //CurrentGetByIdJob.ConfigureAwait(false);
            var prevJob = CurrentGetByIdJob;
            CurrentGetByIdJob = Task.Run(async () =>
            {
                if (prevJob != null && !prevJob.IsCompleted)
                {
                    await prevJob;
                }
                return Repository.GetById(id);
            });

            return CurrentGetByIdJob;
        }

        //public Task<IGSAggregate>


        public Task<IAuthUser> Initialize()
        {

            if (this.Model != null)
                return null;

            return Task.Run(async () =>
            {
                GSApp app = null;

                try
                {
                    app = (GSApp)(await GetById(GSAppState.GSAppId));
                }
                catch (DomainError)
                {

                }

                if (app == null)
                {
                    app = (GSApp)(await HandleCommand(new CreateGSApp()));
                    //app = (GSApp)Handler.Handle(new CreateGSApp());
                }
                Context.SetupCurrentUser(app.State.User);

                this.Model = app;
                this.User = Context.CurrentUser;
                return this.User;
                //return app;
            });
        }



        private ISynchronizerService SyncService;
        private IRequestFactory RequestFactory;

        public async Task<ISyncInstance> Synchronize()
        {
            if (SyncService == null)
                SyncService = Kernel.Get<ISynchronizerService>();
            if (RequestFactory == null)
                RequestFactory = Kernel.Get<IRequestFactory>();

            if (User.AccessToken == null)
                await Context.AuthorizeUser();

            var s = new SyncInstance(
                RequestFactory.CreatePullRequest(Model.State.SyncStreams.ToArray()),
                RequestFactory.CreatePushRequest(Model.State.SyncHead),
                Model.State.PhotoUploads.Values.Select(x => RequestFactory.CreatePhotoUploadRequest(x)).ToArray(),
                Model.State.PhotoDownloads.Values.Select(x => RequestFactory.CreatePhotoDownloadRequest(x)).ToArray()
            );

            if (s.PullReq.IsEmpty && s.PushReq.IsEmpty && s.PhotoUploadRequests.Length == 0)
                return null;



            return await _Synchronize(s);
        }

        protected async Task<ISyncInstance> _Synchronize(ISyncInstance s)
        {
            bool handlePull = false;
            IPhotoDownloadRequest[] downloadRequests = s.PhotoDownloadRequests;



            if (!s.PullReq.IsEmpty)
            {
                var pullResp = await s.Pull();
                if (pullResp != null && pullResp.StatusCode == GSStatusCode.OK && pullResp.Streams != null && pullResp.Streams.Count > 0)
                {
                    Handler.AttachAggregates(pullResp);
                    handlePull = true;
                    if (s.PushReq.IsEmpty)
                    {
                        Handler.Handle(new Pull(s));
                        downloadRequests = Model.State.PhotoDownloads.Values.Select(x => RequestFactory.CreatePhotoDownloadRequest(x)).ToArray();
                    }
                }
            }
            if (!s.PushReq.IsEmpty)
            {
                if (handlePull)
                    s.Merge();

                var pushResp = await s.Push();
                if (pushResp != null && pushResp.StatusCode == GSStatusCode.OK)
                {
                    if (handlePull)
                    {
                        Handler.Handle(new Pull(s));
                        downloadRequests = Model.State.PhotoDownloads.Values.Select(x => RequestFactory.CreatePhotoDownloadRequest(x)).ToArray();
                    }
                    Handler.Handle(new Push(s));
                }
            }
            if (s.PhotoUploadRequests.Length > 0)
            {
                var responses = await s.UploadPhotos();
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK).Select(x => new CompletePhotoUpload(x.Photo)).ToArray();
                if (successes.Length > 0)
                    Handler.Handle(new StreamSegment(Model.State.Id, successes));
            }
            if (downloadRequests.Length > 0)
            {
                var responses = await s.DownloadPhotos(downloadRequests);
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK).Select(x => new CompletePhotoDownload(x.Photo)).ToArray();
                if (successes.Length > 0)
                    Handler.Handle(new StreamSegment(Model.State.Id, successes));
            }
            return s;

        }




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
                return _Gardens.Where(x => x.User.Id == user.Id);
            return _Gardens;

        }


        public IObservable<IGardenViewModel> CurrentGardens(IAuthUser user = null)
        {


            //Func<Guid?, IEnumerable<UserState>> f = UIPersistence.GetUsers;

            //var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            Guid? id = null;
            if (user != null)
                id = user.Id;

            var current = UIPersistence.GetUsers(id)
                .ToObservable()
                .Select(x => new GardenViewModel(x, this));

            return current;

        }





        public IObservable<IPlantActionViewModel> CurrentPlantActions(PlantState state, Guid? PlantActionId = null)
        {


            //Func<Guid?, Guid?, Guid?, IEnumerable<PlantActionState>> f = UIPersistence.GetActions;

            //var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            var current = UIPersistence.GetActions(PlantActionId, state.Id, null)
                .ToObservable()
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


            var current = UIPersistence.GetPlants(null, null, user.Id)
                .ToObservable()
                .Select(x =>
                {
                    var p = new PlantViewModel(x.Item1,
                        x.Item2 != null ? new ScheduleViewModel(x.Item2, ScheduleType.WATERING, this) : null,
                        x.Item3 != null ? new ScheduleViewModel(x.Item3, ScheduleType.FERTILIZING, this) : null,
                        this);

                    return p;
                });

            return current;

        }




        IObservable<Tuple<ScheduleCreated, ScheduleSet>> _Schedules;
        public IObservable<IScheduleViewModel> FutureSchedules(Guid plantId)
        {


            if (_Schedules == null)
            {
                var schedule = Bus.Listen<IEvent>()
                    .OfType<ScheduleCreated>();

                var plant = Bus.Listen<IEvent>()
                    .OfType<ScheduleSet>();



                _Schedules = Observable.CombineLatest(schedule, plant, (u, g) => Tuple.Create(u, g))
                    .Where(x =>
                    {
                        return x.Item1.AggregateId == x.Item2.ScheduleId;
                    })
                    .DistinctUntilChanged();
            }


            return _Schedules
                .Where(x =>
                {
                    return x.Item2.AggregateId == plantId;
                })
                .Select(x => new ScheduleViewModel(x.Item1.AggregateState, x.Item2.Type, this));


        }











        public IObservable<IPlantViewModel> FuturePlants(IAuthUser user)
        {
            return Bus.Listen<IEvent>()
               .OfType<PlantCreated>()
               .Where(x =>
               {
                   return x.UserId == user.Id;
               })
               .Select(x => new PlantViewModel(x.AggregateState, null, null, this));
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


        private IDisposable ButtonsAddSubscription = Disposable.Empty;
        private IDisposable ButtonsRemoveSubscription = Disposable.Empty;

        private void UpdateAppBar(IReadOnlyReactiveList<IButtonViewModel> x = null)
        {
            this._AppBarButtons.RemoveRange(0, this._AppBarButtons.Count);
            if (x != null)
            {
                this._AppBarButtons.AddRange(x);
                ButtonsAddSubscription.Dispose();
                ButtonsRemoveSubscription.Dispose();
                ButtonsAddSubscription = x.ItemsAdded.Subscribe(y =>
                {
                    this._AppBarButtons.Add(y);
                });
                ButtonsRemoveSubscription = x.ItemsRemoved.Subscribe(y =>
                {
                    this._AppBarButtons.Remove(y);
                });

            }
        }

        private void UpdateMenuItems(IReadOnlyReactiveList<IMenuItemViewModel> x = null)
        {
            this._AppBarMenuItems.RemoveRange(0, this._AppBarMenuItems.Count);
            if (x != null)
                this._AppBarMenuItems.AddRange(x);
        }


        protected ReactiveList<IButtonViewModel> _AppBarButtons = new ReactiveList<IButtonViewModel>();
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
        }

        protected ReactiveList<IMenuItemViewModel> _AppBarMenuItems = new ReactiveList<IMenuItemViewModel>();
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get { return _AppBarMenuItems; }
        }


        public PageOrientation _Orientation;
        public PageOrientation Orientation
        {
            get { return _Orientation; }
            set { this.RaiseAndSetIfChanged(ref _Orientation, value); }
        }

        private ReactiveCommand _PageOrientationChangedCommand;
        public IReactiveCommand PageOrientationChangedCommand
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

        public IScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType)
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

        public virtual IAddEditPlantViewModel AddPlantViewModelFactory(PlantState state)
        {
            throw new NotImplementedException();
        }


        public virtual IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm)
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






        public IGSAppViewModel App
        {
            get { return this; }
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





}

