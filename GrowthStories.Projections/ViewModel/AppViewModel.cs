
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
using EventStore.Persistence;

namespace Growthstories.UI.ViewModel
{




    public class AppViewModel : ReactiveObject, IGSAppViewModel
    {

        private string _UrlPath;
        public string UrlPath
        {
            get
            {
                return _UrlPath;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _UrlPath, value);
            }
        }


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

        IEndpoint _Endpoint;
        public IEndpoint Endpoint
        {
            get { return _Endpoint ?? (_Endpoint = Kernel.Get<IEndpoint>()); }
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

        protected ITransportEvents _Transporter;
        protected ITransportEvents Transporter
        {
            get { return _Transporter ?? (_Transporter = Kernel.Get<ITransportEvents>()); }
        }

        protected IUIPersistence _UIPersistence;
        protected IUIPersistence UIPersistence
        {
            get { return _UIPersistence ?? (_UIPersistence = Kernel.Get<IUIPersistence>()); }
        }


        public IDictionary<Guid, PullStream> SyncStreams
        {
            get
            {
                if (Model != null)
                    return Model.State.SyncStreamDict;
                return new Dictionary<Guid, PullStream>();
            }
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
        //GSViewLocator _ViewLocator;
        //public GSViewLocator ViewLocator
        //{
        //    get
        //    {
        //        if (_ViewLocator == null)
        //        {
        //            _ViewLocator = new GSViewLocator();
        //        }
        //        return _ViewLocator;
        //    }
        //}

        public AppViewModel()
        {




            var resolver = RxApp.MutableResolver;

            this.Resolver = resolver;
            
            resolver.RegisterConstant(this, typeof(IScreen));
            resolver.RegisterConstant(this.Router, typeof(IRoutingState));


            //this.Router.NavigationStack.R

            this.Router.CurrentViewModel
                .Select(x =>
                {
                    var xx = x as IHasAppBarButtons;
                    if (xx != null)
                        return xx.WhenAnyValue(y => y.AppBarButtons);
                    return Observable.Return(new ReactiveList<IButtonViewModel>());
                })
                .Switch()
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateAppBar(x ?? new ReactiveList<IButtonViewModel>()));


            this.Router.CurrentViewModel
                .Select(x =>
                {
                    var xx = x as IHasMenuItems;
                    if (xx != null)
                        return xx.WhenAnyValue(y => y.AppBarMenuItems);
                    return Observable.Return(new ReactiveList<IMenuItemViewModel>());
                })
                .Switch()
                .Subscribe(x => this.UpdateMenuItems(x ?? new ReactiveList<IMenuItemViewModel>()));

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
                 .Switch().ObserveOn(RxApp.MainThreadScheduler)
                 .ToProperty(this, x => x.ProgressIndicatorIsVisible, out this._ProgressIndicatorIsVisible, true, RxApp.MainThreadScheduler);



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

            this.Router.CurrentViewModel
                //.OfType<IControlsPageOrientation>()
                .Select(x =>
                {
                    var xx = x as IControlsBackButton;
                    if (xx != null)
                        return xx.WhenAnyValue(y => y.CanGoBack);
                    return Observable.Return(true);
                })
                .Switch()
                .ToProperty(this, x => x.CanGoBack, out this._CanGoBack, true);

            resolver.Register(() => ResetSupport(() => new GardenViewModel(null, this)), typeof(IGardenViewModel));
            resolver.Register(() => ResetSupport(() => new FriendsViewModel(this)), typeof(FriendsViewModel));
            resolver.Register(() => ResetSupport(() => new NotificationsViewModel(this)), typeof(INotificationsViewModel));
            resolver.Register(() => ResetSupport(() => new SettingsViewModel(this)), typeof(ISettingsViewModel));

            resolver.RegisterLazySingleton(() => new AboutViewModel(this), typeof(IAboutViewModel));



            resolver.RegisterLazySingleton(() => new SearchUsersViewModel(Transporter, this), typeof(SearchUsersViewModel));



            //resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));


            ShowPopup = new ReactiveCommand();
            SynchronizeCommand = new ReactiveCommand();

            this.SynchronizeCommand.Subscribe(x =>
            {
                //this.CanSynchronize = false;
                ShowPopup.Execute(this.SyncPopup);
            });
            var syncResult = this.SynchronizeCommand.RegisterAsyncTask(async (_) => await this.SyncAll());
            syncResult.Subscribe(x =>
            {
                //this.CanSynchronize = true;
                App.ShowPopup.Execute(null);
            });

            this.SyncResults = syncResult;

        }

        public IReactiveCommand ShowPopup { get; private set; }
        public IReactiveCommand SynchronizeCommand { get; private set; }
        public IObservable<Tuple<AllSyncResult, GSStatusCode?>> SyncResults { get; protected set; }

        private IPopupViewModel _SyncPopup;
        public IPopupViewModel SyncPopup
        {
            get
            {
                if (_SyncPopup == null)
                {
                    _SyncPopup = new PopupViewModel()
                    {
                        Caption = "Synchronizing",
                        Message = null,
                        IsLeftButtonEnabled = false
                    };
                }
                return _SyncPopup;
            }
        }

        private object _myGarden;
        private object _myFriends;
        private object _myNotifications;
        private object _mySettings;

        protected object ResetSupport<T>(Func<T> factory) where T : class
        {
            if (typeof(T) == typeof(GardenViewModel))
            {
                if (_myGarden == null)
                    _myGarden = factory();
                return _myGarden;
            }
            if (typeof(T) == typeof(FriendsViewModel))
            {
                if (_myFriends == null)
                    _myFriends = factory();
                return _myFriends;
            }
            if (typeof(T) == typeof(NotificationsViewModel))
            {
                if (_myNotifications == null)
                    _myNotifications = factory();
                return _myNotifications;
            }
            if (typeof(T) == typeof(SettingsViewModel))
            {
                if (_mySettings == null)
                    _mySettings = factory();
                return _mySettings;
            }
            return null;
        }

        public void ResetUI()
        {
            _myGarden = null;
            _myFriends = null;
            _myNotifications = null;
            _mySettings = null;
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

        private bool _IsRegistered;
        public bool IsRegistered
        {
            get
            {
                return _IsRegistered;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _IsRegistered, value);
            }
        }


        private ObservableAsPropertyHelper<bool> _CanGoBack;
        public bool CanGoBack
        {
            get
            {
                return _CanGoBack.Value;
            }
        }

        private IReactiveCommand _BackKeyPressedCommand;
        public IReactiveCommand BackKeyPressedCommand
        {
            get
            {
                return _BackKeyPressedCommand ?? (_BackKeyPressedCommand = new ReactiveCommand());
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

                if (this.InitializeJob == null)
                {
                    await Initialize();
                }
                else if (!this.InitializeJob.IsCompleted)
                {
                    await this.InitializeJob;
                }

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

                if (this.InitializeJob == null)
                {
                    await Initialize();
                }
                else if (!this.InitializeJob.IsCompleted)
                {
                    await this.InitializeJob;
                }

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

                if (this.InitializeJob == null)
                {
                    await Initialize();
                }
                else if (!this.InitializeJob.IsCompleted)
                {
                    await this.InitializeJob;
                }

                if (prevJob != null && !prevJob.IsCompleted)
                {
                    await prevJob;
                }
                return Repository.GetById(id);
            });

            return CurrentGetByIdJob;
        }

        //public Task<IGSAggregate>

        protected Task<IAuthUser> InitializeJob;
        public Task<IAuthUser> Initialize()
        {

            if (this.Model != null)
                return Task.FromResult(this.User);

            this.InitializeJob = Task.Run(() =>
            {
                GSApp app = null;

                var persistence = Kernel.Get<IPersistSyncStreams>();
                persistence.Initialize();

                var uiPersistence = Kernel.Get<IUIPersistence>();
                uiPersistence.Initialize();


                try
                {
                    app = (GSApp)(Repository.GetById(GSAppState.GSAppId));
                }
                catch (DomainError)
                {

                }

                if (app == null)
                {
                    //app = (GSApp)(await HandleCommand(new CreateGSApp()));
                    app = (GSApp)Handler.Handle(new CreateGSApp());
                }

                IAuthUser user = null;

                if (app.State.User == null)
                {
                    var u = Context.GetNewUserCommands();

                    if (u.Item2 == null || u.Item2.Length == 0)
                    {
                        user = u.Item1;
                    }
                    else
                    {
                        var first = u.Item2[0] as CreateUser;
                        if (first == null)
                            throw new InvalidOperationException("Can't create new user");
                        user = ((User)Handler.Handle(first)).State;
                        var counter = 0;
                        foreach (var cmd in u.Item2)
                        {
                            counter++;
                            if (counter == 1)
                                continue;
                            Handler.Handle(cmd);
                        }
                    }
                
                } else {
                    user = app.State.User;
                }

                Context.SetupCurrentUser(user);

                this.Model = app;
                this.User = user;
                this.IsRegistered = false;
                if (user.IsRegistered())         
                    this.IsRegistered = true; 
                return user;
                //return app;
            });
            return InitializeJob;
        }



        public async Task<RegisterResponse> Register(string username, string email, string password)
        {

            IAuthUser user = this.User;

            if (user == null)
            {
                var u = this.Context.GetNewUserCommands();

                foreach (var cmd in u.Item2)
                {
                    await this.HandleCommand(cmd);
                }

                user = u.Item1;
            }

            if (user == null)
            {
                throw new InvalidOperationException("Can't create new user");
            }

            if (user.IsRegistered())
            {
                return RegisterResponse.alreadyRegistered;
            }

            if (User.AccessToken == null)
            {
                try
                {
                    await Context.AuthorizeUser();
                }
                catch
                {
                    return RegisterResponse.connectionerror;
                }
            }

            await this.HandleCommand(new MultiCommand(
                new SetEmail(user.Id, email),
                new SetUsername(user.Id, username),
                new SetPassword(user.Id, password)
            ));

            var R = await this.PushAll();
            if (R.Item1 == AllSyncResult.AllSynced)
            {
                this.IsRegistered = true;
                return RegisterResponse.success;
            }

            else if (R.Item1 == AllSyncResult.SomeLeft)
                return RegisterResponse.tryagain;
            else if (R.Item1 == AllSyncResult.Error)
                return RegisterResponse.emailInUse;

            //for (var i = 0; i < 3; i++)
            //{

            //    var empty = new PullStream[] { };
            //    var s = new SyncInstance(
            //       RequestFactory.CreatePullRequest(empty),
            //       RequestFactory.CreatePushRequest(Model.State.SyncHead)
            //    );

            //    await _Synchronize(s);

            //    if (s.PullResp == null || s.PullResp.StatusCode != GSStatusCode.OK)
            //        return RegisterRespone.emailInUse;
            //}


            return RegisterResponse.success;
        }

        public async Task<GSApp> SignOut(bool createUnregUser = true)
        {
            // Clear db
            if (CurrentHandleJob != null && !CurrentHandleJob.IsCompleted)
            {
                await CurrentHandleJob;
            }
            this.ClearDB();
            Handler.ResetApp();
            this._Model = null;

            // Reset UI
            this.ResetUI();

            GSApp app = null;

            if (createUnregUser)
            {
                await this.Initialize();
                app = this.Model;
            }
            else
            {
                app = (GSApp)Handler.Handle(new CreateGSApp());
                this.Model = app;

            }

            return app;
        }

        public async Task<SignInResponse> SignIn(string email, string password)
        {
            IAuthResponse authResponse = null;
            RemoteUser u = null;
            try
            {
                authResponse = await Context.AuthorizeUser(email, password);
                u = await Transporter.UserInfoAsync(email);
            }
            catch
            {
                return SignInResponse.invalidLogin;
            }


            var app = await SignOut(false);

            Handler.Handle(new AssignAppUser(u.AggregateId, u.Username, password, email));
            Handler.Handle(new SetAuthToken(authResponse.AuthToken));

            this.User = app.State.User;
            this.IsRegistered = true;
            Context.SetupCurrentUser(this.User);

            Handler.Handle(new CreateSyncStream(u.AggregateId, PullStreamType.USER));

            // now we get the user stream AND info on the plants
            await this.SyncAll();
            // now we get the plants too
            await this.SyncAll();





            return SignInResponse.success;


        }

        protected virtual void ClearDB()
        {
            throw new NotImplementedException();
        }


        private ISynchronizerService SyncService;

        private IRequestFactory _RequestFactory;
        private IRequestFactory RequestFactory
        {
            get
            {
                return _RequestFactory ?? (_RequestFactory = Kernel.Get<IRequestFactory>());
            }
        }

        public async Task<ISyncInstance> Synchronize()
        {
            //if (SyncService == null)
            //    SyncService = Kernel.Get<ISynchronizerService>();



            if (User.AccessToken == null)
            {
                try
                {
                    var authResponse = await Context.AuthorizeUser();

                }
                catch
                {
                    return null;
                }

            }


            var syncStreams = Model.State.SyncStreams.ToArray();
            var s = new SyncInstance(
                RequestFactory.CreatePullRequest(syncStreams),
                RequestFactory.CreatePushRequest(Model.State.SyncHead),
                Model.State.PhotoUploads.Values.Select(x => RequestFactory.CreatePhotoUploadRequest(x)).ToArray(),
                null
            );

            if (s.PullReq.IsEmpty && s.PushReq.IsEmpty && s.PhotoUploadRequests.Length == 0)
                return null;



            return await _Synchronize(s);
        }

        public async Task<ISyncInstance> Push()
        {
            //if (SyncService == null)
            //    SyncService = Kernel.Get<ISynchronizerService>();



            if (User.AccessToken == null)
            {
                try
                {
                    var authResponse = await Context.AuthorizeUser();

                }
                catch
                {
                    return null;
                }

            }

            var s = new SyncInstance(
                RequestFactory.CreatePullRequest(null),
                RequestFactory.CreatePushRequest(Model.State.SyncHead),
                Model.State.PhotoUploads.Values.Select(x => RequestFactory.CreatePhotoUploadRequest(x)).ToArray(),
                null
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
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK).Select(x => new CompletePhotoUpload(x) { AncestorId = User.Id }).ToArray();
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
                .Where(x => x.Id != this.User.Id && !x.IsDeleted)
                .Select(x => new GardenViewModel(x, this));

            return current;

        }





        public IObservable<IPlantActionViewModel> CurrentPlantActions(Guid plantId, Guid? PlantActionId = null)
        {


            //Func<Guid?, Guid?, Guid?, IEnumerable<PlantActionState>> f = UIPersistence.GetActions;

            //var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            var current = UIPersistence.GetActions(PlantActionId, plantId, null)
                .ToObservable()
                .Where(x => !x.IsDeleted)
                .Select(x => PlantActionViewModelFactory(x.Type, x));

            return current;

        }




        public IObservable<IPlantActionViewModel> FuturePlantActions(Guid plantId, Guid? PlantActionId = null)
        {

            return Bus.Listen<IEvent>()
                    .OfType<PlantActionCreated>()
                    .Where(x => x.PlantId == plantId)
                    .Select(x => PlantActionViewModelFactory(x.AggregateState.Type, x.AggregateState));
        }


        public IObservable<IPlantViewModel> CurrentPlants(IAuthUser user, Guid? plantId = null)
        {


            var current = UIPersistence.GetPlants(plantId, null, user.Id)
                .ToObservable()
                .Where(x => !x.Item1.IsDeleted)
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




        public virtual IPlantActionViewModel PlantActionViewModelFactory(PlantActionType type, PlantActionState state = null)
        {

            if (type == PlantActionType.MEASURED)
                return new PlantMeasureViewModel(this, state);
            else if (type == PlantActionType.PHOTOGRAPHED)
                return new PlantPhotographViewModel(this, state);
            else
            {
                return new PlantActionViewModel(type, this, state);
            }


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

        private void UpdateAppBar(IReadOnlyReactiveList<IButtonViewModel> x)
        {

            if (x == null)
                return;

            this._AppBarButtons.RemoveRange(0, this._AppBarButtons.Count);

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

        private void UpdateMenuItems(IReadOnlyReactiveList<IMenuItemViewModel> x)
        {
            if (x == null)
                return;
            this._AppBarMenuItems.RemoveRange(0, this._AppBarMenuItems.Count);
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


        public string PageTitle
        {
            get { throw new NotImplementedException(); }
        }

        public string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IAddEditPlantViewModel EditPlantViewModelFactory(IPlantViewModel pvm)
        {
            throw new NotImplementedException();

        }


        public virtual IYAxisShitViewModel YAxisShitViewModelFactory(IPlantViewModel pvm)
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

