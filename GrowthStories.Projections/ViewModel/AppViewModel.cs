
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommonDomain;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;


namespace Growthstories.UI.ViewModel
{


    public class AppViewModel : ReactiveObject, IGSAppViewModel, IEnableLogger
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


        public static Enough.Async.AsyncLock LocationLock = new Enough.Async.AsyncLock();



        public async Task<GSLocation> GetLocation()
        {
            using (var res = await LocationLock.LockAsync())
            {
                var loc = await DoGetLocation();
                if (loc != null)
                {
                    this.Handler.Handle(new AcquireLocation(loc));
                }
                return loc;
            }
        }


        public virtual Task<GSLocation> DoGetLocation()
        {
            throw new NotImplementedException();
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


        public IDictionary<Guid, PullStream> SyncStreams
        {
            get
            {
                if (Model != null)
                    return Model.State.SyncStreamDict;
                return new Dictionary<Guid, PullStream>();
            }
        }

        private bool isUIPersistenceInitialized = false;
        private readonly IUIPersistence _UIPersistence;
        protected IUIPersistence UIPersistence
        {
            get
            {
                if (!isUIPersistenceInitialized)
                {
                    _UIPersistence.Initialize();
                    isUIPersistenceInitialized = true;
                }
                return _UIPersistence;
            }
        }


        public Guid GardenId { get; set; }


        public IMutableDependencyResolver Resolver { get; protected set; }



        public virtual bool HasPayed()
        {
            return true; // should override this
        }

        public IRoutingState Router { get { return _Router; } }

        private readonly IUserService Context;
        private readonly IDispatchCommands Handler;
        protected readonly IGSRepository Repository;
        private readonly ITransportEvents Transporter;
        private readonly IIAPService IIAPService;
        private readonly IScheduleService Scheduler;
        private readonly IRequestFactory RequestFactory;
        private readonly IRoutingState _Router;

        public AppViewModel(
            IMutableDependencyResolver resolver,
            IUserService context,
            IDispatchCommands handler,
            IGSRepository repository,
            ITransportEvents transporter,
            IUIPersistence uiPersistence,
            IIAPService iiapService,
            IScheduleService scheduler,
            IRequestFactory requestFactory,
            IRoutingState router,
            IMessageBus bus
         )
        {
            this.Context = context;
            this.Handler = handler;
            this.Repository = repository;
            this.Transporter = transporter;
            this._UIPersistence = uiPersistence;
            this.IIAPService = iiapService;
            this.RequestFactory = requestFactory;
            this._Router = router;
            this.Resolver = resolver;
            this.Bus = bus;
            this.Scheduler = scheduler;

            //resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));

            // COMMANDS
            ShowPopup = new ReactiveCommand();
            SynchronizeCommand = Observable.Return(true).ToCommandWithSubscription(_ => ShowPopup.Execute(this.SyncPopup));
            UISyncFinished = new ReactiveCommand();
            PageOrientationChangedCommand = Observable.Return(true).ToCommandWithSubscription(x =>
            {

                try
                {
                    this.Orientation = (PageOrientation)x;

                }
                catch
                {

                }

            });


            BackKeyPressedCommand = new ReactiveCommand();
            InitializeJobStarted = new ReactiveCommand();
            SignedOut = new ReactiveCommand();

            var syncResult = this.SynchronizeCommand.RegisterAsyncTask(async (_) => await this.SyncAll());

            syncResult.Subscribe(x =>
            {
                //this.CanSynchronize = true;
                App.ShowPopup.Execute(null);
                UISyncFinished.Execute(x);
            });

            // we need to set these immediately to have the defaults in place when starting up

            this.SyncResults = syncResult;

            Bootstrap();
        }


        public bool NotifiedOnBadConnection { get; set; }


        private void Bootstrap()
        {

            var resolver = Resolver;

            var vmChanged = this.Router.CurrentViewModel.DistinctUntilChanged();

            vmChanged
               .OfType<IControlsAppBar>()
               .Select(x => x.WhenAny(y => y.AppBarMode, y => y.GetValue()).StartWith(x.AppBarMode))
               .Switch()
               .ToProperty(this, x => x.AppBarMode, out this._AppBarMode, ApplicationBarMode.MINIMIZED);

            vmChanged
                 .OfType<IControlsAppBar>()
                 .Select(x => x.WhenAny(y => y.AppBarIsVisible, y => y.GetValue()).StartWith(x.AppBarIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.AppBarIsVisible, out this._AppBarIsVisible, true);


            vmChanged
                .Select(x =>
                {
                    //this.Log().Info("VM changed MAINBUTTONS");
                    var xx = x as IHasAppBarButtons;
                    if (xx != null)
                        return xx.WhenAnyValue(y => y.AppBarButtons);
                    //return null;
                    return Observable.Return(new ReactiveList<IButtonViewModel>());
                })
                .Switch()
                .Throttle(TimeSpan.FromMilliseconds(200))
                .DistinctUntilChanged()
                .Do(x =>
                {
                    //this.Log().Info("from throttle MAINBUTTONS changed, count={0}, text={1}", x.Count, string.Join(",", x.Select(y => y.Text)));
                })
                .ToProperty(this, x => x.AppBarButtons, out _AppBarButtons);
            //.Subscribe(x => this.UpdateAppBar(x ?? ));

            //this.WhenAnyValue(x => x.AppBarButtons).Where(x => x != null).Subscribe(x => this.Log().Info("AppViewModel MAINBUTTONS changed, count={0}", x.Count));

            vmChanged
                .Select(x =>
                {
                    var xx = x as IHasMenuItems;
                    if (xx != null)
                        return xx.WhenAnyValue(y => y.AppBarMenuItems);
                    //return null;
                    return Observable.Return(new ReactiveList<IMenuItemViewModel>());
                })
                .Switch()
                .Throttle(TimeSpan.FromMilliseconds(200))
                .DistinctUntilChanged()
                .ToProperty(this, x => x.AppBarMenuItems, out _AppBarMenuItems);



            vmChanged
                 .OfType<IControlsSystemTray>()
                 .Select(x => x.WhenAnyValue(y => y.SystemTrayIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.SystemTrayIsVisible, out this._SystemTrayIsVisible, false);

            vmChanged
               .OfType<IRequiresNetworkConnection>()
               .Select(x => x.WhenAnyValue(y => y.NoConnectionAlert))
               .Switch()
               .Subscribe(x =>
               {
                   if (!this.HasDataConnection)
                       this.ShowPopup.Execute(x ?? this.DefaultNoConnectionAlert);
               });


            vmChanged
                 .OfType<IControlsProgressIndicator>()
                 .Select(x => x.WhenAnyValue(y => y.ProgressIndicatorIsVisible))
                 .Switch().ObserveOn(RxApp.MainThreadScheduler)
                 .ToProperty(this, x => x.ProgressIndicatorIsVisible, out this._ProgressIndicatorIsVisible, true, RxApp.MainThreadScheduler);

            vmChanged
                .Select(x =>
                {
                    var xx = x as IControlsPageOrientation;
                    if (xx != null)
                        return xx.WhenAnyValue(y => y.SupportedOrientations);
                    return Observable.Return(SupportedPageOrientation.Portrait);
                })
                .Switch()
                .ToProperty(this, x => x.SupportedOrientations, out this._SupportedOrientations, SupportedPageOrientation.Portrait);



            this.WhenAnyValue(x => x.MyGarden)
                .Where(x => x != null)
                .Subscribe(x => Scheduler.ScheduleGarden(x));

        }

        #region COMMANDS
        public IReactiveCommand ShowPopup { get; private set; }
        public IReactiveCommand SynchronizeCommand { get; private set; }
        public IReactiveCommand UISyncFinished { get; private set; }
        public IReactiveCommand SignedOut { get; private set; }

        public IObservable<Tuple<AllSyncResult, GSStatusCode?>> SyncResults { get; protected set; }

        public IReactiveCommand BackKeyPressedCommand { get; private set; }

        public IReactiveCommand InitializeJobStarted { get; private set; }

        public IReactiveCommand PageOrientationChangedCommand { get; private set; }



        #endregion

        private IPopupViewModel _SyncPopup;
        public IPopupViewModel SyncPopup
        {
            get
            {
                if (_SyncPopup == null)
                {
                    _SyncPopup = new ProgressPopupViewModel();
                }
                return _SyncPopup;
            }
        }

        private IPopupViewModel _DefaultNoConnectionAlert;
        public IPopupViewModel DefaultNoConnectionAlert
        {
            get
            {
                if (_DefaultNoConnectionAlert == null)
                {
                    _DefaultNoConnectionAlert = new PopupViewModel()
                    {
                        IsLeftButtonEnabled = true,
                        Caption = "The feature you are requesting requires connectivity."
                    };
                }
                return _DefaultNoConnectionAlert;
            }
        }





        private IGardenViewModel _MyGarden;
        public IGardenViewModel MyGarden
        {
            get
            {
                return _MyGarden;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _MyGarden, value);
            }
        }


        public IMainViewModel CreateMainViewModel()
        {

            var scheduler = RxApp.TaskpoolScheduler;
            var settings = Observable.Start(() => new SettingsViewModel(this), scheduler);
            var add = Observable.Start(() => this.EditPlantViewModelFactory(null), scheduler);

            var gardenObs = Observable.Start(() =>
            {
                return new GardenViewModel(
                        this.WhenAnyValue(x => x.User),
                        true,
                        this,
                        IIAPService,
                        settings,
                        add
                    );
            }
            , scheduler).ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            {
                this.MyGarden = x;
            });

            //myGarden.ObserveOn()
            //var gardenObs = this.WhenAnyValue(x => x.MyGarden).Where(x => x != null);

            var notifications = Observable.Start(() => new NotificationsViewModel(gardenObs, this), scheduler);
            var friends = Observable.Start(() => new FriendsViewModel(this), scheduler);

            return new MainViewModel(gardenObs, notifications, friends, this);
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

        private string _UserEmail;
        public string UserEmail
        {
            get
            {
                return _UserEmail;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _UserEmail, value);
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


        //private ObservableAsPropertyHelper<bool> _CanGoBack;
        //public bool CanGoBack
        //{
        //    get
        //    {
        //        return _CanGoBack.Value;
        //    }
        //}

        private int AutoSyncCount = 0;
        private bool AutoSyncEnabled = true;


        private void SubscribeForAutoSync()
        {
            this.ListenTo<EventBase>().Subscribe(e =>
            {
                if (e.AggregateId == this.User.Id
                    || e.AncestorId == this.User.Id
                    || e.EntityId == this.User.Id
                    || e.StreamAncestorId == this.User.Id
                    || e.StreamEntityId == this.User.Id)
                {
                    // this is an async call, so it will not block
                    //PossiblyAutoSync();
                }
            });
        }


        //
        // Autosync if there are not two or more unfinished autosyncs
        //
        // This method should be called whenever we detect changes we
        // would like to sync immediately, and also possibly periodically
        //
        public async Task _PossiblyAutoSync()
        {
            if (AutoSyncCount < 2)
            {
                try
                {
                    var guid = Guid.NewGuid().ToString();
                    this.Log().Info("Autosyncing (debugId: " + guid + ")");
                    this.AutoSyncCount++;
                    await this.SyncAll();
                    this.Log().Info("Autosync finished (debugId: " + guid + ")");

                }
                finally
                {
                    this.AutoSyncCount--;
                }
            }

        }


        public void PossiblyAutoSync()
        {
            if (!HasDataConnection || !AutoSyncEnabled)
            {
                return;
            }

            // before triggering the actual autosync,
            // wait for a few seconds for more important
            // processing to finish
            if (AutoSyncCount < 2)
            {
                Task.Delay(1000 * 5).ContinueWith(__ => _PossiblyAutoSync());
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



        protected Task<IAuthUser> InitializeJob;
        public Task<IAuthUser> Initialize()
        {

            if (this.Model != null)
                return Task.FromResult(this.User);

            this.InitializeJob = Task.Run(() =>
            {
                GSApp app = null;


                InitializeJobStarted.Execute(null);

                //var kernel = Kernel;
                //var persistence = kernel.Get<IPersistSyncStreams>();
                //persistence.Initialize();

                //var uiPersistence = kernel.Get<IUIPersistence>();
                //uiPersistence.Initialize();

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

                }
                else
                {
                    user = app.State.User;
                }

                Context.SetupCurrentUser(user);

                this.Model = app;
                this.User = user;
                this.UserEmail = user.Email;

                // did not now how to execute code
                // in the RxApp.MainThreadScheduler
                // without this kludge 
                //   -- JOJ 4.1.2014
                var kludge = new ReactiveCommand();
                kludge
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        this.IsRegistered = false;
                        if (user.IsRegistered)
                        {
                            this.IsRegistered = true;
                        }
                    });
                kludge.Execute(null);

                this.ListenTo<InternalRegistered>(app.State.Id)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                        {
                            this.IsRegistered = true;
                        });

                this.ListenTo<Registered>(user.Id)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.IsRegistered = true);

                SubscribeForAutoSync();

                kludge = new ReactiveCommand();
                kludge
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ =>
                {
                    var us = GetUserState(user.Id);
                    if (us != null)
                    {
                        this.GSLocationServicesEnabled = us.LocationEnabled;
                    }
                    else
                    {
                        this.GSLocationServicesEnabled = false;
                    }
                });
                kludge.Execute(null);

                this.ListenTo<LocationEnabledSet>(user.Id)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                        this.GSLocationServicesEnabled = x.LocationEnabled
                        );

                this.LastLocation = app.State.LastLocation;
                this.ListenTo<LocationAcquired>(app.State.Id)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.LastLocation = x.Location);

                return user;
            });
            return InitializeJob;
        }


        protected IObservable<T> ListenTo<T>(Guid id = default(Guid)) where T : IEvent
        {
            var allEvents = this.Bus.Listen<IEvent>().OfType<T>();
            if (id == default(Guid))
                return allEvents;
            else
                return allEvents.Where(x => x.AggregateId == id);
        }


        public async Task<RegisterResponse> Register(string username, string email, string password)
        {
            await EnsureUserInitialized();

            if (IsRegistered)
            {
                return RegisterResponse.alreadyRegistered;
            }

            if (await PrepareAuthorizedUser() != GSStatusCode.OK)
            {
                return RegisterResponse.connectionerror;
            }

            // make sure stuff is synchronized before registering, so
            // that there will be no sync conflict after the registration
            var asr = await this.SyncAll();
            if (asr.Item1 != AllSyncResult.AllSynced)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
                return RegisterResponse.connectionerror;
            }

            APIRegisterResponse resp = await Transporter.RegisterAsync(username, email, password);

            if (resp.HttpStatus != System.Net.HttpStatusCode.OK)
            {
                // could of course be also internal server error
                // etc but for users we present these as connection errors
                return RegisterResponse.connectionerror;
            }

            if (resp.RegisterStatus == RegisterStatus.EMAIL_EXISTS)
            {
                return RegisterResponse.emailInUse;
            }

            if (resp.RegisterStatus == RegisterStatus.USERNAME_EXISTS)
            {
                return RegisterResponse.usernameInUse;
            }

            InternalRegisterAppUser cmd = new InternalRegisterAppUser(User.Id, username, password, email);
            await this.HandleCommand(cmd);

            // synchronizing is not really required here, 
            // but it is probably nice to still to do it
            /*
            asr = await this.SyncAll();
            if (asr.Item1 != AllSyncResult.AllSynced)
            {
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
            */

            return RegisterResponse.success;
        }


        private bool _GSLocationServicesEnabled;
        public bool GSLocationServicesEnabled
        {
            get
            {
                return _GSLocationServicesEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _GSLocationServicesEnabled, value);
            }
        }



        private async Task EnsureUserInitialized()
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

            GSApp app = null;

            if (createUnregUser)
            {
                await this.Initialize();
                app = this.Model;
                Router.NavigateAndReset.Execute(CreateMainViewModel());

            }
            else
            {
                app = (GSApp)Handler.Handle(new CreateGSApp());
                this.Model = app;
            }



            this.SignedOut.Execute(null);
            return app;
        }


        public async Task<SignInResponse> SignIn(string email, string password)
        {
            try
            {
                AutoSyncEnabled = false;
                return await _SignIn(email, password);
            }
            finally
            {
                AutoSyncEnabled = true;
            }
        }


        private async Task<SignInResponse> _SignIn(string email, string password)
        {
            IAuthResponse authResponse = null;
            RemoteUser u = null;

            try
            {
                authResponse = await Context.AuthorizeUser(email, password);

                // server returns 401 when user is not found
                if (authResponse.StatusCode == GSStatusCode.AUTHENTICATION_REQUIRED)
                {
                    return SignInResponse.invalidEmail;
                }

                // server returns 403 when password is incorrect
                if (authResponse.StatusCode == GSStatusCode.FORBIDDEN)
                {
                    return SignInResponse.invalidPassword;
                }

                // anything else (except OK) means there is either a connection error
                // or something broken at the backend
                if (authResponse.StatusCode != GSStatusCode.OK)
                {
                    return SignInResponse.connectionerror;
                }

                u = await Transporter.UserInfoAsync(email);
            }
            catch
            {
                // this would be caused by Transporter.USerInfoAsync failing
                return SignInResponse.connectionerror;
            }

            var app = await SignOut(false);

            Handler.Handle(new AssignAppUser(u.AggregateId, u.Username, password, email));
            Handler.Handle(new InternalRegisterAppUser(u.AggregateId, u.Username, password, email));
            Handler.Handle(new SetAuthToken(authResponse.AuthToken));

            this.User = app.State.User;
            this.IsRegistered = true;
            Context.SetupCurrentUser(this.User);

            Handler.Handle(new CreateSyncStream(u.AggregateId, PullStreamType.USER));

            // we pull our _own_ user stream, get info on the plants and the followed users
            await this.SyncAll();
            // we pull our _own_ plant streams, we pull followed users' streams and get info on followed users' plants
            await this.SyncAll();
            // we pull followed user' plants
            await this.SyncAll();

            Router.NavigateAndReset.Execute(CreateMainViewModel());

            return SignInResponse.success;
        }


        protected virtual void ClearDB()
        {
            throw new NotImplementedException();
        }





        public static Enough.Async.AsyncLock SynchronizeLock = new Enough.Async.AsyncLock();





        // Special push for creating user
        //
        private async Task<ISyncInstance> PushCreateUser()
        {
            var s = new SyncInstance
            (
               RequestFactory.CreatePullRequest(null),
               RequestFactory.CreatePushRequest(Model.State.SyncHead),
               new IPhotoUploadRequest[0],
               null
            );

            return await this._Synchronize(s);
        }



        // Tries to prepare an authorized user
        //
        //  - Pushes UserCreated if necessary
        //  - Obtains AuthToken if necessary
        //  
        //  ( May add later auth check with server)
        //
        public async Task<GSStatusCode> PrepareAuthorizedUser()
        {


            // if we have not yet pushed the CreateUser event,
            // do that before obtaining auth token
            var res = RequestFactory.GetNextPushEvent(Model.State.SyncHead);

            if (res.Item1 is UserCreated)
            {
                var s = await PushCreateUser();

                if (s.Status != SyncStatus.OK)
                {
                    // this should not happen
                    this.Log().Warn("failed to push CreateUser for user id " + App.User.Id);
                    return GSStatusCode.FAIL;
                }
            }

            if (User.AccessToken == null)
            {
                var authResponse = await Context.AuthorizeUser();
                return authResponse.StatusCode;
            }

            return GSStatusCode.OK;
        }



        // Despite the lock, only SyncAll should call this function
        // (except for unit tests)
        public async Task<ISyncInstance> Synchronize()
        {
            using (var r = await SynchronizeLock.LockAsync())
            {
                return await UnsafeSynchronize();
            }
        }


        // Do a synchronization cycle once
        // (1) pull -> (2) push -> (3) photo download
        //
        private async Task<ISyncInstance> UnsafeSynchronize()
        {
            var code = await PrepareAuthorizedUser();
            if (code != GSStatusCode.OK)
            {
                return new SyncInstance(SyncStatus.AUTH_ERROR);
            }

            var syncStreams = Model.State.SyncStreams.ToArray();
            var s = new SyncInstance
            (
                RequestFactory.CreatePullRequest(syncStreams),
                RequestFactory.CreatePushRequest(Model.State.SyncHead),
                Model.State.PhotoUploads.Values.Select(x => RequestFactory.CreatePhotoUploadRequest(x)).ToArray(),
                null
            );

            // pullrequest should really never be empty
            if (s.PullReq.IsEmpty && s.PushReq.IsEmpty && s.PhotoUploadRequests.Length == 0)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return new SyncInstance(SyncStatus.PULL_EMPTY_ERROR);
            }

            return await _Synchronize(s);
        }


        public static Enough.Async.AsyncLock _SynchronizeLock = new Enough.Async.AsyncLock();


        protected async Task<ISyncInstance> _Synchronize(ISyncInstance s)
        {
            using (var release = await _SynchronizeLock.LockAsync())
            {
                return await _UnsafeSynchronize(s);
            }
        }


        private async Task<ISyncInstance> _UnsafeSynchronize(ISyncInstance s)
        {
            bool handlePull = false;
            IPhotoDownloadRequest[] downloadRequests = s.PhotoDownloadRequests;

            if (!s.PullReq.IsEmpty)
            {
                var pullResp = await s.Pull();
                if (pullResp != null
                    && pullResp.StatusCode == GSStatusCode.OK
                    && pullResp.Streams != null
                    && pullResp.Streams.Count > 0)
                {
                    Handler.AttachAggregates(pullResp);
                    handlePull = true;
                    if (s.PushReq.IsEmpty)
                    {
                        Handler.Handle(new Pull(s));
                        downloadRequests = Model.State.PhotoDownloads.Values.Select(x => RequestFactory.CreatePhotoDownloadRequest(x)).ToArray();
                    }
                }

                if (pullResp == null || pullResp.StatusCode != GSStatusCode.OK)
                {
                    // this suggests access token has expired, next sync will use new one
                    // should handle this better, but this will do for now 
                    if (pullResp.StatusCode == GSStatusCode.AUTHENTICATION_REQUIRED)
                    {
                        User.AccessToken = null;
                    }
                    s.Status = SyncStatus.PULL_ERROR;
                    return s;
                }
            }


            if (!s.PushReq.IsEmpty)
            {
                if (handlePull)
                {
                    try
                    {
                        s.Merge();
                    }
                    catch
                    {
                        if (Debugger.IsAttached) { Debugger.Break(); };
                        s.Status = SyncStatus.MERGE_ERROR;
                        return s;
                    }
                }

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

                if (pushResp == null || (pushResp.StatusCode != GSStatusCode.OK
                    && pushResp.StatusCode != GSStatusCode.VERSION_TOO_LOW))
                {
                    s.Status = SyncStatus.PUSH_ERROR;
                    return s;
                }
            }

            if (s.PhotoUploadRequests.Length > 0)
            {
                var responses = await s.UploadPhotos();
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK).Select(x => new CompletePhotoUpload(x) { AncestorId = User.Id }).ToArray();
                if (successes.Length > 0)
                    Handler.Handle(new StreamSegment(Model.State.Id, successes));

                foreach (var resp in responses)
                {
                    if (resp.StatusCode != GSStatusCode.OK)
                    {
                        s.Status = SyncStatus.PHOTOUPLOAD_ERROR;
                        return s;
                    }
                }
            }

            if (downloadRequests.Length > 0)
            {
                var responses = await s.DownloadPhotos(downloadRequests);
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK).Select(x => new CompletePhotoDownload(x.Photo)).ToArray();
                if (successes.Length > 0)
                    Handler.Handle(new StreamSegment(Model.State.Id, successes));

                foreach (var resp in responses)
                {
                    if (resp.StatusCode != GSStatusCode.OK)
                    {
                        s.Status = SyncStatus.PHOTODOWNLOAD_ERROR;
                        return s;
                    }
                }
            }
            s.Status = SyncStatus.OK;

            return s;
        }


        IObservable<IGardenViewModel> _Gardens;
        public IObservable<IGardenViewModel> FutureGardens(Guid? userId = null)
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
                    .Select(x => new GardenViewModel(Observable.Return(x.Item2.AggregateState), false, this));
            }

            if (userId != null)
                return _Gardens.Where(x => x.User.Id == userId);
            return _Gardens;
        }


        public IEnumerable<Guid> GetCurrentFollowers(Guid userId)
        {
            return GetUserState(userId).Friends.Keys.AsEnumerable();
        }


        private UserState GetUserState(Guid userId)
        {
            var search = UIPersistence.GetUsers(null).Where(x => x.Id == userId);
            if (search.Count() > 0)
            {
                return search.First();
            }
            else
            {
                return null;
            }
        }


        //public UserState GetUserState()
        //{
        //    var search = UIPersistence.GetUsers(null).Where(x => x.Id == App.User.Id);
        //    return search.First();
        //}


        public IObservable<IGardenViewModel> CurrentGardens(Guid? userId = null)
        {
            return UIPersistence.GetUsers(userId)
                .ToObservable()
                .Where(x => !x.IsDeleted && (User == null || User.Id != x.Id))
                .Select(x => new GardenViewModel(Observable.Return(x), false, this));
        }


        public IObservable<IPlantActionViewModel> CurrentPlantActions(
            Guid PlantId,
            PlantActionType? type = null,
            int? limit = null,
            bool? isOrderAsc = null
            )
        {

            //Func<Guid?, Guid?, Guid?, IEnumerable<PlantActionState>> f = UIPersistence.GetActions;

            //var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            var current = UIPersistence.GetActions(PlantId: PlantId, type: type, limit: limit, isOrderAsc: isOrderAsc)
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

        public IPlantViewModel GetSinglePlant(Guid plantId)
        {
            return PlantViewModelFactory(Observable.Start(() => UIPersistence
                .GetPlants(plantId, null, null)
                .ToObservable()
                .Where(x => !x.Item1.IsDeleted)
                , RxApp.TaskpoolScheduler).Merge());
        }

        public IObservable<IPlantViewModel> CurrentPlants(Guid? userId = null, Guid? plantId = null)
        {
            if (plantId != null)
                return Return(GetSinglePlant(plantId.Value));
            return Observable.Start(() => UIPersistence.GetPlants(null, null, userId).ToObservable(), RxApp.TaskpoolScheduler)
                .Merge()
                .Where(x => !x.Item1.IsDeleted)
                .Select(x => PlantViewModelFactory(Observable.Return(x)));
            //this.Log().Info("CurrentPlants: userId: {0}, plantId: {1}", userId, plantId);
            //var plants = Observable.Defer(() =>
            //{
            //    this.Log().Info("CurrentPlants Task started");
            //})
            //.Do(x => this.Log().Info("CurrentPlants Task ended"))
            //.Where(x => !x.Item1.IsDeleted).Publish().RefCount();

            //IObservable<IPlantViewModel> r = null;
            //if (plantId != null)
            //{
            //    this.Log().Info("Returning single plant");
            //    r = Return(PlantViewModelFactory(plants));
            //}
            //else
            //{
            //    r = plants.Select(x => PlantViewModelFactory(Observable.Return(x)));
            //}
            //this.Log().Info("CurrentPlants: end");

            //return r;
        }

        public static IObservable<T> Return<T>(T value)
        {
            return Observable.Create<T>(o =>
            {
                o.OnNext(value);
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<IPlantViewModel> FuturePlants(Guid userId)
        {
            return Bus.Listen<IEvent>()
               .OfType<PlantCreated>()
               .Where(x =>
               {
                   return x.UserId == userId;
               })
               .Select(x => PlantViewModelFactory(Observable.Return(Tuple.Create(x.AggregateState, (ScheduleState)null, (ScheduleState)null))));
        }



        protected virtual IPlantViewModel PlantViewModelFactory(IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable)
        {
            return new PlantViewModel(stateObservable, this);
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


        private ObservableAsPropertyHelper<IReadOnlyReactiveList<IButtonViewModel>> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons.Value;
            }
        }

        private ObservableAsPropertyHelper<IReadOnlyReactiveList<IMenuItemViewModel>> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {
                return _AppBarMenuItems.Value;
            }
        }


        public PageOrientation _Orientation;
        public PageOrientation Orientation
        {
            get { return _Orientation; }
            set { this.RaiseAndSetIfChanged(ref _Orientation, value); }
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

        public bool HasDataConnection
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
        }

        public bool EnsureDataConnection()
        {
            if (!HasDataConnection)
                ShowPopup.Execute(DataConnectionNotification);
            return HasDataConnection;
        }

        private IPopupViewModel _DataConnectionNotification;
        private IPopupViewModel DataConnectionNotification
        {
            get
            {
                return _DataConnectionNotification ?? (_DataConnectionNotification = new PopupViewModel()
                {
                    Caption = "Data connection required",
                    Message = "Sharing requires a data connection. Please enable one in your phone's settings and try again.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                });
            }
        }


        public IGSAppViewModel App
        {
            get { return this; }
        }

        private GSLocation _LastLocation;
        public GSLocation LastLocation
        {
            get
            {
                return _LastLocation;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _LastLocation, value);
            }
        }


        private bool _PhoneLocationServicesEnabled;
        public bool PhoneLocationServicesEnabled
        {
            get
            {
                return _PhoneLocationServicesEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PhoneLocationServicesEnabled, value);
            }
        }


        //
        // ONLY FOR TESTING
        // not necessarily safe, do not call from app code
        //
        public async Task<ISyncInstance> Push()
        {

            if (await PrepareAuthorizedUser() != GSStatusCode.OK)
            {
                return null;
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


        public virtual void UpdatePhoneLocationServicesEnabled()
        {
            throw new NotImplementedException();
        }





    }


}

