
using System;
using System.Collections.Generic;
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
        private readonly ISynchronizer Synchronizer;
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
            ISynchronizer synchronizer,
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
            this.Synchronizer = synchronizer;

            //resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));

            // COMMANDS
            SetDismissPopupAllowed = new ReactiveCommand();
            ShowPopup = new ReactiveCommand();
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
            SignedOut = new ReactiveCommand();


            Bootstrap();
        }


        public bool NotifiedOnBadConnection { get; set; }


        public void NotifyImageDownloadFailed()
        {

            if (this.NotifiedOnBadConnection)
            {
                return;
            }

            this.NotifiedOnBadConnection = true;

            PopupViewModel pvm;
            if (!HasDataConnection)
            {
                this.Log().Info("images failed to load because of broken data connection");
                pvm = new PopupViewModel()
                {
                    Caption = "No data connection",
                    Message = "Photos of followed user's plants may not be displayed, because you don't have a data connection.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                };
            }
            else
            {
                this.Log().Info("images failed to load because of broken data connection");

                pvm = new PopupViewModel()
                {
                    Caption = "Failed to load images",
                    Message = "Some photos of followed user's plants failed to load. This may be caused by an invalid data connection. Growth Stories will try to load them later.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                };
            }

            this.ShowPopup.Execute(pvm);
        }


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

            this.WhenAnyValue(x => x.Model)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    Synchronizer.SubscribeForAutoSync(x.State);
                });

        }

        #region COMMANDS
        public IReactiveCommand ShowPopup { get; private set; }
        public IReactiveCommand SetDismissPopupAllowed { get; private set; }
        public IReactiveCommand SignedOut { get; private set; }
        public IReactiveCommand BackKeyPressedCommand { get; private set; }
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

                // try to get a previously created application from the repository
                try
                {
                    app = (GSApp)(Repository.GetById(GSAppState.GSAppId));
                }
                catch (DomainError)
                {
                    // this means it's the first time the application's run
                    // so let's create a new application
                    app = (GSApp)Handler.Handle(new CreateGSApp());

                }
                catch (Exception)
                {
                    // something really unexcpected happened
                    throw;
                }

                IAuthUser user = app.State.User;

                if (user == null)
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

                //SubscribeForAutoSync();

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


        public bool RegisterCancelRequested { get; set; }


        private Enough.Async.AsyncLock RegisterLock = new Enough.Async.AsyncLock();


        public async Task<RegisterResponse> Register(string username, string email, string password)
        {
            using (var res = await RegisterLock.LockAsync())
            {
                return await _Register(username, email, password);
            }
        }


        private async Task<RegisterResponse> _Register(string username, string email, string password)
        {
            RegisterCancelRequested = false;

            await EnsureUserInitialized();

            if (IsRegistered)
            {
                return RegisterResponse.alreadyRegistered;
            }

            if (await this.Synchronizer.PrepareAuthorizedUser(Model.State.SyncHead) != GSStatusCode.OK)
            {
                if (RegisterCancelRequested)
                {
                    return RegisterResponse.canceled;
                }
                this.Log().Info("registration: connection error when trying to authorize");
                return RegisterResponse.connectionerror;
            }

            // make sure stuff is synchronized before registering, so
            // that there will be no sync conflict after the registration
            var asr = await this.Synchronizer.SyncAll(Model.State);
            if (asr.Item1 != AllSyncResult.AllSynced)
            {
                if (RegisterCancelRequested)
                {
                    return RegisterResponse.canceled;
                }
                this.Log().Info("registration: authorize worked but sync failed");
                return RegisterResponse.connectionerror;
            }

            if (RegisterCancelRequested)
            {
                return RegisterResponse.canceled;
            }

            SetDismissPopupAllowed.Execute(false);
            var ret = await _FinishRegistration(username, email, password);
            SetDismissPopupAllowed.Execute(true);
            return ret;
        }


        private async Task<RegisterResponse> _FinishRegistration(string username, string email, string password)
        {
            APIRegisterResponse resp = await Transporter.RegisterAsync(username, email, password);

            // this is a little bit messy as we don't know 
            // whether the request went through to the server
            // luckily this is quite a rare situation
            if (resp.HttpStatus != System.Net.HttpStatusCode.OK)
            {
                if (RegisterCancelRequested)
                {
                    return RegisterResponse.canceled;
                }
                // could of course be also internal server error
                // etc but for users we present these as connection errors
                return RegisterResponse.connectionerror;
            }

            if (resp.RegisterStatus != RegisterStatus.OK && RegisterCancelRequested)
            {
                return RegisterResponse.canceled;
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


        //
        // skipLock = true is used when the caller already has the relevant lock
        // 
        public async Task<GSApp> SignOut(bool createUnregUser = true, bool skipLock = false)
        {
            // Clear db
            if (CurrentHandleJob != null && !CurrentHandleJob.IsCompleted)
            {
                await CurrentHandleJob;
            }
            GSApp app = null;

            //using (Synchronizer.DisableAutoSync())
            //{
            if (skipLock)
            {
                app = await _SignOut(createUnregUser);
            }
            else
            {
                using (await Synchronizer.AcquireLock())
                {
                    app = await _SignOut(createUnregUser);
                }
            }
            Synchronizer.SubscribeForAutoSync(app.State);
            //}

            this.SignedOut.Execute(null);
            return app;
        }



        private async Task<GSApp> _SignOut(bool createUnregUser)
        {
            GSApp app = null;

            this.ClearDB();
            Handler.ResetApp();
            this._Model = null;

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

            return app;
        }


        private Enough.Async.AsyncLock SignInLock = new Enough.Async.AsyncLock();

        public async Task<SignInResponse> SignIn(string email, string password)
        {
            // user can press signin, dismiss popup and press 
            // signin so we better make signins are synchronous
            using (var res = await SignInLock.LockAsync())
            {

                // note that an autosync may already be in progress
                // we are dealing with that with a separate lock inside
                // _SignIn(...)
                return await _SignIn(email, password);

            }
        }


        public bool SignInCancelRequested { get; set; }


        private async Task<SignInResponse> _SignIn(string email, string password)
        {
            SignInCancelRequested = false;

            IAuthResponse authResponse = null;
            RemoteUser u = null;

            try
            {
                authResponse = await Context.AuthorizeUser(email, password);
                if (SignInCancelRequested)
                {
                    return SignInResponse.canceled;
                }

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
                if (SignInCancelRequested)
                {
                    return SignInResponse.canceled;
                }
                // this would be caused by Transporter.USerInfoAsync failing
                return SignInResponse.connectionerror;
            }

            try
            {
                // don't allow dismissing popup
                SetDismissPopupAllowed.Execute(false);

                //var empty = Disposable.Empty;
                // do not go here while any sync operation is in progress
                //using (var res = await _SynchronizeLock.LockAsync())
                using (await Synchronizer.AcquireLock())
                {
                    var app = await SignOut(false, true);

                    Handler.Handle(new AssignAppUser(u.AggregateId, u.Username, password, email));
                    Handler.Handle(new InternalRegisterAppUser(u.AggregateId, u.Username, password, email));
                    Handler.Handle(new SetAuthToken(authResponse.AuthToken));

                    this.User = app.State.User;
                    this.IsRegistered = true;
                    Context.SetupCurrentUser(this.User);

                    Handler.Handle(new CreateSyncStream(u.AggregateId, PullStreamType.USER));
                }

                // important: the upcoming SyncAll operations must 
                // not be inside the above using block, as the asynclocks
                /// are not re-entrant 

                // (1) we pull our _own_ user stream, get info on the plants and the followed users
                // (2) we pull our _own_ plant streams, we pull followed users' streams and get info on followed users' plants
                // (3) we pull followed user' plants
                for (int i = 0; i < 3; i++)
                {
                    var res = (await this.Synchronizer.SyncAll(Model.State)).Item1;
                    if (res == AllSyncResult.Error)
                    {
                        return SignInResponse.messCreated;
                    }
                }

                Router.NavigateAndReset.Execute(CreateMainViewModel());
                return SignInResponse.success;
            }
            finally
            {
                SetDismissPopupAllowed.Execute(true);
            }

        }


        protected virtual void ClearDB()
        {
            throw new NotImplementedException();
        }


        private Task<ISyncInstance> PushCreateUser()
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<AllSyncResult, GSStatusCode?>> Synchronize()
        {
            if (!HasDataConnection)
            {
                PopupViewModel pvm = new PopupViewModel()
                {
                    Caption = "Data connection required",
                    Message = "Synchronizing requires a data connection. Please enable one in your phone's settings and try again.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                };

                ShowPopup.Execute(pvm);
                return null;
            }
            return this.Synchronizer.SyncAll(Model.State);
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
                    .Select(x =>
                        {
                            this.Log().Info("instantiating new gardenviewmodel");
                            return new GardenViewModel(Observable.Return(x.Item2.AggregateState), false, this);
                        });
            }

            if (userId != null)
                return _Gardens.Where(x => x.User.Id == userId);
            return _Gardens;
        }


        // Return GUIDs to the users 
        // currently App.User is currently following 
        //
        public IEnumerable<Guid> GetCurrentPYFs()
        {
            var us = GetUserState(User.Id);
            if (us == null)
            {
                this.Log().Warn("userstate was null");
                return new List<Guid>();
            }
            var friends = us.Friends;
            if (friends == null)
            {
                this.Log().Warn("userstate.Friends is null");
                return new List<Guid>();
            }

            var ret = friends.Keys.AsEnumerable();
            return ret;
        }


        // Get UserState for given user 
        //
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





        public virtual void UpdatePhoneLocationServicesEnabled()
        {
            throw new NotImplementedException();
        }





    }


}

