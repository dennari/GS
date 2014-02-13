
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
using Growthstories.UI.Services;
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
                return _SupportedOrientations != null ? _SupportedOrientations.Value : SupportedPageOrientation.Portrait;
            }
        }


        private IMainViewModel _MainVM;
        public IMainViewModel MainVM
        {
            get
            {
                return _MainVM;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _MainVM, value);
            }
        }

        private IPlantSingularViewModel _PlantSingularVM;
        public IPlantSingularViewModel PlantSingularVM
        {
            get
            {
                return _PlantSingularVM;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PlantSingularVM, value);
            }
        }



        public virtual void HandleApplicationActivated()
        {
            this.UpdatePhoneLocationServicesEnabled();
        }


        public virtual void UpdatePhoneLocationServicesEnabled()
        {

        }


        // Get the current location
        //
        // Important: should only be called from the main thread
        //
        public async Task<GSLocation> GetLocation()
        {
            var kludge = new ReactiveCommand();
            using (var res = await LocationLock.LockAsync())
            {
                var loc = await DoGetLocation();
                if (loc != null)
                {
                    await this.Handler.Handle(new AcquireLocation(loc));
                }
                return loc;
            }
        }


        protected virtual Task<GSLocation> DoGetLocation()
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

        private IGSRoutingState _Router;
        public IGSRoutingState Router
        {
            get
            {
                return _Router ?? (_Router = new GSRoutingState());
            }
            //set
            //{
            //    this.RaiseAndSetIfChanged(ref _Router, value);
            //}
        }

        IRoutingState IScreen.Router
        {
            get
            {
                return (IRoutingState)this.Router;
            }
            //set
            //{
            //    this.RaiseAndSetIfChanged(ref _Router, value);
            //}
        }
        //private IRoutingState _Router;
        //public IRoutingState Router
        //{
        //    get
        //    {
        //        return _Router;
        //    }
        //    private set
        //    {
        //        this.RaiseAndSetIfChanged(ref _Router, value);
        //    }
        //}

        private readonly IUserService Context;
        private readonly IDispatchCommands Handler;
        private readonly ITransportEvents Transporter;
        private readonly IIAPService IIAPService;
        private readonly IScheduleService Scheduler;
        private readonly ISynchronizer Synchronizer;
        private readonly IRequestFactory RequestFactory;

        private readonly AsyncLock RegisterLock = new AsyncLock();
        private readonly AsyncLock SignInLock = new AsyncLock();
        public static readonly AsyncLock LocationLock = new AsyncLock();

        public AppViewModel(
            IMutableDependencyResolver resolver,
            IUserService context,
            IDispatchCommands handler,
            ITransportEvents transporter,
            IUIPersistence uiPersistence,
            IIAPService iiapService,
            IScheduleService scheduler,
            ISynchronizer synchronizer,
            IRequestFactory requestFactory,
            //IRoutingState router, // let's lazyload this, it's relatively heavy
            IMessageBus bus
         )
        {

            //this.Log().Info("AppViewModel constructor begins {0}", GSAutoSuspendApplication.LifeTimer.ElapsedMilliseconds);

            this.Context = context;
            this.Handler = handler;
            this.Transporter = transporter;
            this._UIPersistence = uiPersistence;
            this.IIAPService = iiapService;
            this.RequestFactory = requestFactory;
            //this._Router = router;
            this.Resolver = resolver;
            this.Bus = bus;
            this.Scheduler = scheduler;
            this.Synchronizer = synchronizer;

            //resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));

            // COMMANDS

            MainWindowLoadedCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                {
                    var mvm = x as IMainViewModel;
                    if (mvm != null)
                        MainVM = mvm;
                    var svm = x as IPlantSingularViewModel;
                    if (svm != null)
                        PlantSingularVM = svm;

                    if (!Bootstrapped)
                    {
                        Bootstrapped = true;
                        Bootstrap(x as IGSViewModel);
                    }
                });

            Initialize();

            //Bootstrap();

            //NavigateAndResetStopwatch = new Stopwatch();
            //this.Router.NavigateAndReset.Subscribe(_ =>
            // {
            //     this.Log().Info("navigate and reset");
            //NavigateAndResetStopwatch.Restart();
            // });

        }
        public bool NavigatingBack { get; set; }

        private bool Bootstrapped = false;
        //public Stopwatch NavigateAndResetStopwatch {get; set;} 


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
                    Message = "Photos of some plants may not be displayed, because you don't have a data connection.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                };
            }

            {
                this.Log().Info("images failed to load because of broken data connection");

                pvm = new PopupViewModel()
                {
                    Caption = "Failed to load images",
                    Message = "Some photos of plants failed to load. This may be caused by an invalid data connection. Growth Stories will try to load them later.",
                    IsLeftButtonEnabled = true,
                    LeftButtonContent = "OK"
                };
            }

            this.ShowPopup.Execute(pvm);
        }


        protected virtual void Bootstrap(IGSViewModel defaultVM)
        {

            this.Log().Info("Bootstrap");
            var resolver = Resolver;
            //this.Router = new GSRoutingState();
            //this.Router.Navigate.Execute(this);

            var vmChanged = this.Router.CurrentViewModel
                .Select(x =>
                {
                    if (x != null)
                        return x as IGSViewModel;
                    if (defaultVM is IMainViewModel)
                        return (IGSViewModel)MainVM;
                    return (IGSViewModel)PlantSingularVM;
                })
                .DistinctUntilChanged();
            //.Do(x => this.Log().Info("Router ViewModel changed to {0}", x));



            vmChanged
               .OfType<IControlsAppBar>()
               .Select(x => x.WhenAnyValue(y => y.AppBarMode))
               .Switch()
                //   .Do(x => this.Log().Info("ApplicationBarMode {0}", x))
               .ToProperty(this, x => x.AppBarMode, out this._AppBarMode, ApplicationBarMode.MINIMIZED);

            vmChanged
                 .OfType<IControlsAppBar>()
                 .Select(x => x.WhenAnyValue(y => y.AppBarIsVisible))
                 .Switch()
                //    .Do(x => this.Log().Info("AppBarIsVisible {0}", x))
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
                //.Do(x =>this.Log().Info("AppBarButtons changed, count={0}", x != null ? x.Count : 0))
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
                 .Do(x =>
                 {
                     this.Log().Info("SystemTrayIsVisible {0}", x);

                 })
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
                .Do(x => this.Log().Info("MyGarden set"))
                .Subscribe(x => Scheduler.ScheduleGarden(x));

            this.WhenAnyValue(x => x.Model)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    Synchronizer.SubscribeForAutoSync(x.State);
                });

            this.WhenAnyValue(x => x.User)
               .Where(x => x != null)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Do(x => this.Log().Info("User set"))
               .Subscribe(x =>
               {
                   this.IsRegistered = x.IsRegistered;
                   this.GSLocationServicesEnabled = x.LocationEnabled;
                   //UserState user = null;
                   //try
                   //{
                   //    var ret = await GetById(x.Id);
                   //    if (ret == null)
                   //    {
                   //        this.Log().Warn("could not get userstate for user {0}", x.Id);
                   //    }
                   //    user = ret.State as UserState;
                   //    if (user == null)
                   //    {
                   //        this.Log().Warn("could not get userstate for user {0}", x.Id);
                   //    }
                   //}
                   //catch { this.Log().Warn("could not get userstate for user {0} (exception)", x.Id); }

                   //if (user == null)
                   //{
                   //    this.Log().Info("setting gslocationservices enabled via appstate to {0}", x.LocationEnabled);
                   //    this.GSLocationServicesEnabled = x.LocationEnabled;
                   //}
                   //else
                   //{
                   //    this.Log().Info("setting gslocationservices enabled via userstate to {0}", user.LocationEnabled);
                   //    this.GSLocationServicesEnabled = user.LocationEnabled;
                   //}

               });

        }

        #region COMMANDS


        private IReactiveCommand _PageOrientationChangedCommand;
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

        private IReactiveCommand _ShowPopup;
        public IReactiveCommand ShowPopup
        {
            get
            {
                return _ShowPopup ?? (_ShowPopup = new ReactiveCommand());
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

        private IReactiveCommand _MainWindowLoadedCommand;
        public IReactiveCommand MainWindowLoadedCommand
        {
            get
            {
                return _MainWindowLoadedCommand ?? (_MainWindowLoadedCommand = new ReactiveCommand());
            }

        }

        private IReactiveCommand _SetDismissPopupAllowedCommand;
        public IReactiveCommand SetDismissPopupAllowedCommand
        {
            get
            {
                return _SetDismissPopupAllowedCommand ?? (_SetDismissPopupAllowedCommand = new ReactiveCommand());
            }
        }

        private IReactiveCommand _SignedOut;
        protected IReactiveCommand SignedOut
        {
            get
            {
                return _SignedOut ?? (_SignedOut = new ReactiveCommand());
            }

        }



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


        private IMainViewModel _CurrentMainViewModel = null;
        public IMainViewModel CreateMainViewModel()
        {

            Func<IGardenViewModel> gardenF = () =>
            {
                return new GardenViewModel(
                        this.WhenAnyValue(x => x.User),
                        true,
                        this,
                        IIAPService,
                        () => new SettingsViewModel(this),
                        () => this.EditPlantViewModelFactory(null)
                    );

            };

            var gardenObs = this.WhenAnyValue(x => x.MyGarden).Where(x => x != null);

            Func<INotificationsViewModel> notificationsF = () => new NotificationsViewModel(gardenObs, this);
            Func<FriendsViewModel> friendsF = () => new FriendsViewModel(this);

            //myGarden.ObserveOn()

            //var notifications = Observable.Start(() => new NotificationsViewModel(gardenObs, this), scheduler);
            //var friends = Observable.Start(() => new FriendsViewModel(this), scheduler);

            if (_CurrentMainViewModel != null)
                _CurrentMainViewModel.Dispose();
            _CurrentMainViewModel = new MainViewModel(gardenF, notificationsF, friendsF, this);
            _CurrentMainViewModel.WhenAnyValue(x => x.GardenVM).Where(x => x != null).Take(1).Subscribe(x => this.MyGarden = x);
            return _CurrentMainViewModel;
        }

        //public IMainViewModel CreateMainViewModel()
        //{
        //    return new MainViewModel(
        //        () => (IGardenViewModel)null,
        //        () => (INotificationsViewModel)null,
        //        () => (FriendsViewModel)null,
        //        this
        //        );
        //}

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




        public Task<IGSAggregate> HandleCommand(IAggregateCommand x)
        {
            // this is important!
            if (!x.AncestorId.HasValue)
                this.SetIds(x);
            return Handler.Handle(x);
        }


        public Task<IGSAggregate> HandleCommand(MultiCommand x)
        {
            return Handler.Handle(x);
        }



        private List<IDisposable> subs = new List<IDisposable>();


        protected Task<IAuthUser> InitializeJob;
        public Task<IAuthUser> Initialize()
        {

            if (this.Model != null)
                return Task.FromResult(this.User);

            this.InitializeJob = Task.Run(async () =>
            {
                GSApp app = null;

                // try to get a previously created application from the repository
                try
                {
                    app = (GSApp)(await Handler.GetById(GSAppState.GSAppId));
                    this.Log().Info("found previous GSApp");
                }
                catch (DomainError)
                {
                    // this means it's the first time the application's run
                    // so let's create a new application

                }
                catch (Exception e)
                {
                    // something really unexcpected happened
                    this.Log().DebugExceptionExtended("Tried to get GSApp in Init", e);

                    throw;
                }
                if (app == null)
                {
                    this.Log().Info("creating new GSApp");
                    app = (GSApp)(await Handler.Handle(new CreateGSApp()));
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
                        user = ((User)(await Handler.Handle(first))).State;
                        var counter = 0;
                        foreach (var cmd in u.Item2)
                        {
                            counter++;
                            if (counter == 1)
                                continue;
                            await Handler.Handle(cmd);
                        }
                    }

                }

                Context.SetupCurrentUser(user);

                this.Model = app;
                this.User = user;
                this.UserEmail = user.Email;
                this.LastLocation = app.State.LastLocation;

                SetupSubscriptions(app, user);

                return user;
            });
            return InitializeJob;
        }


        private void SetupSubscriptions(GSApp app, IAuthUser user)
        {
            this.Log().Info("info setting up subscriptions for user {0}", user.Id);
            foreach (var d in subs)
            {
                d.Dispose();
            }
            subs.Clear();

            subs.Add(this.ListenTo<InternalRegistered>(app.State.Id)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                this.UserEmail = x.Email;
                this.Log().Info("setting user registered via InternalRegistered event {0}", user.Id);
                this.IsRegistered = true;
            }));

            subs.Add(this.ListenTo<Registered>(user.Id)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.Log().Info("setting user registered via Registered event {0}", user.Id);
                    this.IsRegistered = true;
                    this.UserEmail = x.Email;
                }));

            subs.Add(this.ListenTo<EmailSet>(user.Id)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.Log().Info("setting user email via Registered event {0}", user.Id);
                    this.UserEmail = x.Email;
                }));

            this.Log().Info("subscribing new listener for location services enabled {0}", user.Id);
            subs.Add(this.ListenTo<LocationEnabledSet>(user.Id)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.Log().Info("setting location services enabled {0}", user.Id);
                    this.GSLocationServicesEnabled = x.LocationEnabled;
                }
                ));

            subs.Add(this.ListenTo<LocationAcquired>(app.State.Id)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.LastLocation = x.Location));
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



        private IPlantViewModel _SelectedPlant;
        public IPlantViewModel SelectedPlant
        {
            get
            {
                return _SelectedPlant;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedPlant, value);
            }
        }

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

            SetDismissPopupAllowedCommand.Execute(false);
            var ret = await _FinishRegistration(username, email, password);
            SetDismissPopupAllowedCommand.Execute(true);
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
            //Handler.ResetApp();
            Growthstories.UI.Services.GSViewLocator.Instance.Reset();

            this._Model = null;

            if (createUnregUser)
            {
                await this.Initialize();
                app = this.Model;
                var mvm = CreateMainViewModel();

                Router.Reset();

                this.MainWindowLoadedCommand.Execute(mvm);
            }
            else
            {
                app = (GSApp)(await HandleCommand(new CreateGSApp()));
                this.Model = app;
            }

            return app;
        }

        private bool SignInInProgress = false;



        public async Task<SignInResponse> SignIn(string email, string password)
        {
            // user can press signin, dismiss popup and press 
            // signin so we better make signins are synchronous

            var signInLock = await SignInLock.LockAsync();
            SignInInProgress = true;
            using (Disposable.Create(() =>
            {
                SignInInProgress = false;
                signInLock.Dispose();
            }))
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
                if (SignInCancelRequested)
                {
                    return SignInResponse.canceled;
                }
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
                this.Log().Info("signin no more allowing cancel");

                // don't allow dismissing popup
                SetDismissPopupAllowedCommand.Execute(false);

                //var empty = Disposable.Empty;
                // do not go here while any sync operation is in progress
                //using (var res = await _SynchronizeLock.LockAsync())

                using (await Synchronizer.AcquireLock())
                {
                    var app = await SignOut(false, true);

                    await Handler.Handle(new AssignAppUser(u.AggregateId, u.Username, password, email));

                    this.User = app.State.User;
                    SetupSubscriptions(app, this.User);

                    await Handler.Handle(new InternalRegisterAppUser(u.AggregateId, u.Username, password, email));
                    await Handler.Handle(new SetAuthToken(authResponse.AuthToken));

                    this.IsRegistered = true;
                    Context.SetupCurrentUser(this.User);

                    await Handler.Handle(new CreateSyncStream(u.AggregateId, PullStreamType.USER));
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

                var mvm = CreateMainViewModel();
                Router.Reset();
                this.MainWindowLoadedCommand.Execute(mvm);

                return SignInResponse.success;
            }
            finally
            {
                SetDismissPopupAllowedCommand.Execute(true);
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


        IObservable<IGardenViewModel> FuturePYFsObservable;
        public IObservable<IGardenViewModel> FuturePYFs(Guid? userId = null)
        {

            if (FuturePYFsObservable == null)
            {
                FuturePYFsObservable = Bus.Listen<IEvent>()
                     .OfType<BecameFollower>()
                     .Where(x => x.AggregateId == this.User.Id && !SignInInProgress)
                     .SelectMany(x =>
                     {


                         return Observable.Start(() =>
                         {


                             // ok, so we first check if we already have the user's stream
                             // (this can happen if we've been following the same user earlier)
                             UserState state = UIPersistence.GetUsers(new[] { x.Target }).FirstOrDefault();
                             if (state == null)
                             {
                                 // we have to wait for the GardenAdded event

                                 return new GardenViewModel(
                                     Bus.Listen<IEvent>()
                                        .OfType<GardenAdded>()
                                        .Where(y => y.AggregateId == x.Target)
                                        .Select(y => y.AggregateState)
                                        .Take(1)
                                        .Do(y => this.Log().Info("instantiating FUTURE LAZY pyf {0}", x.Target))
                                        ,
                                    false,
                                    this);

                             }
                             this.Log().Info("instantiating FUTURE pyf {0}", x.Target);
                             return new GardenViewModel(Observable.Return(state), false, this);




                         }, RxApp.TaskpoolScheduler).Take(1);
                     });
            }
            return FuturePYFsObservable;

        }

        public IObservable<IGardenViewModel> CurrentPYFs(Guid? userId = null)
        {

            var currentUserState = UIPersistence.GetUsers(new[] { User.Id }).FirstOrDefault();
            if (currentUserState == null || currentUserState.Friends.Count == 0)
                return null;


            return UIPersistence.GetUsers(currentUserState.Friends.Keys.ToArray())
                .ToObservable()
                .Where(x => !x.IsDeleted && (User == null || User.Id != x.Id))
                .Select(x =>
                {
                    this.Log().Info("instantiating CURRENT pyf {0}", x.Id);

                    return new GardenViewModel(Observable.Return(x), false, this);
                });
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
                    .Where(x => x.PlantId == plantId && !SignInInProgress)
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
                .Select(x => PlantViewModelFactory(Observable.Return(x)))
                .Publish()
                .RefCount();

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
                   return x.UserId == userId && !SignInInProgress;
               })
               .Select(x =>
               {
                   this.Log().Info("instantiating futureplant {0}", x.AggregateId);

                   return PlantViewModelFactory(Observable.Return(Tuple.Create(x.AggregateState, (ScheduleState)null, (ScheduleState)null)));
               });
        }



        protected virtual IPlantViewModel PlantViewModelFactory(IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable)
        {
            return new PlantViewModel(stateObservable, this);
        }

        public virtual ISearchUsersViewModel SearchUsersViewModelFactory(IFriendsViewModel friendsVM)
        {
            return new SearchUsersViewModel(Transporter, friendsVM, this);
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
                        return x.Item1.AggregateId == x.Item2.ScheduleId && !SignInInProgress;
                    })
                    .DistinctUntilChanged();
            }


            return _Schedules
                .Where(x => x.Item2.AggregateId == plantId)
                .Select(x =>
                {
                    this.Log().Info("instantiating futureschedule {0}", x.Item1.AggregateId);

                    return new ScheduleViewModel(x.Item1.AggregateState, x.Item2.Type, this);
                });


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
            get { return _AppBarIsVisible != null ? _AppBarIsVisible.Value : false; }
        }

        protected ObservableAsPropertyHelper<bool> _SystemTrayIsVisible;
        public bool SystemTrayIsVisible
        {
            get { return _SystemTrayIsVisible != null ? _SystemTrayIsVisible.Value : false; }
        }

        protected ObservableAsPropertyHelper<bool> _ProgressIndicatorIsVisible;
        public bool ProgressIndicatorIsVisible
        {
            get { return _ProgressIndicatorIsVisible != null ? _ProgressIndicatorIsVisible.Value : false; }
        }

        protected ObservableAsPropertyHelper<ApplicationBarMode> _AppBarMode;
        public ApplicationBarMode AppBarMode
        {
            get { return _AppBarMode != null ? _AppBarMode.Value : ApplicationBarMode.MINIMIZED; }
        }


        private ObservableAsPropertyHelper<IReadOnlyReactiveList<IButtonViewModel>> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons != null ? _AppBarButtons.Value : null;
            }
        }

        private ObservableAsPropertyHelper<IReadOnlyReactiveList<IMenuItemViewModel>> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {
                return _AppBarMenuItems != null ? _AppBarMenuItems.Value : null;
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


        public void Dispose() { } // never going to happen
    
    }


}

