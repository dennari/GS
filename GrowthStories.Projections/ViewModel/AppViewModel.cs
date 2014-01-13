
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
using System.Diagnostics;

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Reactive.Disposables;
using EventStore.Persistence;
using System.Net.NetworkInformation;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

using EventStore.Logging;


namespace Growthstories.UI.ViewModel
{


    public class AppViewModel : ReactiveObject, IGSAppViewModel
    {

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(AppViewModel));

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
            get
            {
                if (_Router == null)
                {
                    _Router = new RoutingState();
                }
                return _Router;
            }
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
        public IUIPersistence UIPersistence
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


        public virtual bool HasPayed()
        {
            return true; // should override this
        }


        public AppViewModel()
        {

            this.Resolver = RxApp.MutableResolver;

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

            DeleteTileCommand = new ReactiveCommand();
            MyGardenCreatedCommand = new ReactiveCommand();
            BackKeyPressedCommand = new ReactiveCommand();
            InitializeJobStarted = new ReactiveCommand();
            SignedOut = new ReactiveCommand();
            IAPCommand = new ReactiveCommand();
            AfterIAPCommand = new ReactiveCommand();


            InitializeJobStarted.Take(1).Subscribe(_ => Bootstrap());

            var syncResult = this.SynchronizeCommand.RegisterAsyncTask(async (_) => await this.SyncAll());

            syncResult.Subscribe(x =>
            {
                //this.CanSynchronize = true;
                App.ShowPopup.Execute(null);
                UISyncFinished.Execute(x);
            });

            // we need to set these immediately to have the defaults in place when starting up
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


            this.SyncResults = syncResult;

            //LoadMainVM();
        }

        private void Bootstrap()
        {

            var resolver = Resolver;

            resolver.RegisterConstant(this, typeof(IScreen));
            resolver.RegisterConstant(this.Router, typeof(IRoutingState));

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
            resolver.Register(() => ResetSupport(() => new NotificationsViewModel(_myGarden as IGardenViewModel, this)), typeof(INotificationsViewModel));
            resolver.Register(() => ResetSupport(() => new SettingsViewModel(this)), typeof(ISettingsViewModel));

            resolver.RegisterLazySingleton(() => new AboutViewModel(this), typeof(IAboutViewModel));
            resolver.RegisterLazySingleton(() => new SearchUsersViewModel(Transporter, this), typeof(SearchUsersViewModel));
        }

        #region COMMANDS
        public IReactiveCommand MyGardenCreatedCommand { get; private set; }
        public IReactiveCommand ShowPopup { get; private set; }
        public IReactiveCommand SynchronizeCommand { get; private set; }
        public IReactiveCommand UISyncFinished { get; private set; }
        public IReactiveCommand SignedOut { get; private set; }

        public IObservable<Tuple<AllSyncResult, GSStatusCode?>> SyncResults { get; protected set; }

        public IReactiveCommand BackKeyPressedCommand { get; private set; }

        public IReactiveCommand InitializeJobStarted { get; private set; }

        public IReactiveCommand PageOrientationChangedCommand { get; private set; }

        public IReactiveCommand DeleteTileCommand { get; private set; }

        public IReactiveCommand IAPCommand { get; private set; }

        public IReactiveCommand AfterIAPCommand { get; private set; }


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
                MyGardenCreatedCommand.Execute(_myGarden);
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

        private int AutoSyncCount = 0;


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
                    PossiblyAutoSync();
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
            
            if (!HasDataConnection)
            {
                return;
            }

            if (AutoSyncCount < 2)
            {
                try
                {
                    var guid = Guid.NewGuid().ToString();
                    Logger.Info("Autosyncing (debugId: " + guid + ")");
                    this.AutoSyncCount++;
                    await this.SyncAll();
                    Logger.Info("Autosync finished (debugId: " + guid + ")");

                } finally {
                    this.AutoSyncCount--;
                }
            } 
           
        }


        public void PossiblyAutoSync()
        {
            // before triggering the actual autosync,
            // wait for a few seconds for more important
            // processing to finish
            Task.Delay(1000 * 5).ContinueWith(__ => _PossiblyAutoSync());
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


        protected virtual IKernel GetKernel()
        {
            return this.Kernel;
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

                var kernel = GetKernel();
                var persistence = kernel.Get<IPersistSyncStreams>();
                persistence.Initialize();

                var uiPersistence = kernel.Get<IUIPersistence>();
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

                }
                else
                {
                    user = app.State.User;
                }

                Context.SetupCurrentUser(user);

                this.Model = app;
                this.User = user;

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

            this.SignedOut.Execute(null);
            return app;
        }


        public async Task<SignInResponse> SignIn(string email, string password)
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

        public static Enough.Async.AsyncLock SynchronizeLock = new Enough.Async.AsyncLock();


        // Do a synchronization cycle once
        // (1) pull -> (2) push -> (3) photo download
        //
        public async Task<ISyncInstance> Synchronize()
        {
            using (var r = await SynchronizeLock.LockAsync())
            {
                return await UnsafeSynchronize();
            }
        }


        private async Task<ISyncInstance> UnsafeSynchronize()
        {
            // obtain access token if required
            if (User.AccessToken == null)
            {
                try
                {
                    var authResponse = await Context.AuthorizeUser();
                }
                catch
                {
                    return new SyncInstance(SyncStatus.AUTH_ERROR);
                }
            }

            // TODO: what if auth token is expired?
            // (this would also have to be considered for other 
            //  requests, so probably should look at a general way to 
            //  implement this)

            var syncStreams = Model.State.SyncStreams.ToArray();

            var s = new SyncInstance
            (
                RequestFactory.CreatePullRequest(syncStreams),
                RequestFactory.CreatePushRequest(Model.State.SyncHead),
                Model.State.PhotoUploads.Values.Select(x => RequestFactory.CreatePhotoUploadRequest(x)).ToArray(),
                null
            );

            // ?? pullrequest should really never be empty
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



        //
        // ONLY FOR TESTING
        // not necessarily safe, do not call from app code
        //
        public async Task<ISyncInstance> Push()
        {
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


        public IGSAppViewModel App
        {
            get { return this; }
        }


    }


}

