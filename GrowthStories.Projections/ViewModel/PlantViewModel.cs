
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.Services;
using ReactiveUI;


namespace Growthstories.UI.ViewModel
{

    public enum DeleteRequestOrigin
    {
        UNKNOWN,
        PARENT,
        SELF
    }


    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {

        #region COMMANDS
        public IObservable<IPlantActionViewModel> PlantActionStream { get; protected set; }

        public IReactiveCommand ShareCommand { get; protected set; }
        public IReactiveCommand TryShareCommand { get; protected set; }
        public IReactiveCommand WateringCommand { get; protected set; }
        //public IObservable<IPlantViewModel> WateringObservable { get; protected set; }

        public IReactiveCommand PhotoCommand { get; protected set; }
        public IReactiveCommand DeleteCommand { get; protected set; }
        public IReactiveCommand DeleteRequestedCommand { get; private set; }

        public IReactiveCommand EditCommand { get; protected set; }
        public IReactiveCommand PinCommand { get; protected set; }
        public IReactiveCommand ScrollCommand { get; protected set; }
        public IReactiveCommand ResetAnimationsCommand { get; protected set; }
        public IReactiveCommand ShowActionList { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; set; }

        #endregion


        private Guid _Id;
        public Guid Id
        {
            get
            {
                return _Id;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _Id, value);
            }
        }


        private Guid _UserId;
        public Guid UserId
        {
            get
            {
                return _UserId;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _UserId, value);
            }
        }

        private IAuthUser _AppUser;
        private IAuthUser AppUser
        {
            get
            {
                return _AppUser;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _AppUser, value);
            }
        }


        public PlantState State { get; protected set; }


        private bool _IsShared;
        public bool IsShared
        {
            get
            {
                return _IsShared;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsShared, value);
            }
        }


        private bool _IsWateringScheduleEnabled;
        public bool IsWateringScheduleEnabled
        {
            get
            {
                return _IsWateringScheduleEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsWateringScheduleEnabled, value);
                this.raisePropertyChanged("ShowWateringScheduler");
                this.ShowTileNotification = IsWateringScheduleEnabled && WateringScheduler != null && WateringScheduler.MissedLateAndOwn;
            }
        }


        private bool _IsFertilizingScheduleEnabled;
        public bool IsFertilizingScheduleEnabled
        {
            get
            {
                return _IsFertilizingScheduleEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsFertilizingScheduleEnabled, value);
                this.raisePropertyChanged("ShowFertilizingScheduler");
            }
        }


        private bool _HasWriteAccess;
        public bool HasWriteAccess
        {
            get
            {
                return _HasWriteAccess;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _HasWriteAccess, value);
            }
        }


        private IPlantActionListViewModel _PlantActionList;
        protected IPlantActionListViewModel PlantActionList
        {
            get
            {
                if (_PlantActionList == null)
                {
                    _PlantActionList = new PlantActionListViewModel(null, App)
                    {
                        Plant = this
                    };
                }
                return _PlantActionList;
            }
        }

        private bool _ShouldBeFullyLoaded;
        public bool ShouldBeFullyLoaded
        {
            get {
                return _ShouldBeFullyLoaded;
            }
            set {
                this.RaiseAndSetIfChanged(ref _ShouldBeFullyLoaded, value);
            }
        }


        public PlantViewModel(IObservable<Tuple<PlantState, ScheduleState, ScheduleState>> stateObservable, IGSAppViewModel app)
            : base(app)
        {
            Loaded = false;
            ShowPlaceHolder = false;

            if (stateObservable == null)
                throw new ArgumentNullException("StateObservable cannot be null");

            this.WateringCommand = new ReactiveCommand();
            this.FertilizingSchedule = new ScheduleViewModel(null, ScheduleType.FERTILIZING, app);

            ResetAnimationsCommand = new ReactiveCommand();
            var canShare = Observable.CombineLatest(
                this.WhenAnyValue(x => x.IsShared),
                App.WhenAnyValue(x => x.IsRegistered),
                (a, b) => a && b);
            this.ShareCommand = new ReactiveCommand(canShare);
            this.DeleteCommand = new ReactiveCommand();
            this.DeleteRequestedCommand = Observable.Return(true).ToCommandWithSubscription(_ =>
                        {
                            var popup = DeleteConfirmation();
                            popup.DismissedObservable
                                .Where(x => x == PopupResult.LeftButton)
                                .Take(1)
                                .Subscribe(__ =>
                                {
                                    this.DeleteCommand.Execute(null);
                                });
                            App.ShowPopup.Execute(popup);
                        });



            var obs = this.DeleteCommand.RegisterAsyncTask(_ =>
            {
                return SendCommand(new DeleteAggregate(this.Id, "plant"));
            }).Publish();
            obs.Connect();

            this.EditCommand = Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(App.EditPlantViewModelFactory(this)));
            this.PinCommand = new ReactiveCommand();
            this.ScrollCommand = new ReactiveCommand();
            this.TryShareCommand = new ReactiveCommand();
            this.ShowActionList = Observable.Return(true).ToCommandWithSubscription(_ => this.Navigate(PlantActionList));


            this.PhotoCommand = Observable.Return(true).ToCommandWithSubscription(_ =>
            {
                var vm = (IPlantPhotographViewModel)CreateEmptyActionVM(PlantActionType.PHOTOGRAPHED);
                vm.PhotoChooserCommand.Execute(null);
                vm.WhenAnyValue(x => x.Photo).Subscribe(x =>
                {
                    if (x != null)
                        vm.AddCommand.Execute(null);
                });
                //vm.AddCommand.Execute(null);
            });


            Observable.CombineLatest(
                     App.WhenAnyValue(x => x.User),
                     stateObservable,
                     (x, y) => Tuple.Create(x, y)
                 )
             .Where(x => x.Item1 != null && x.Item2 != null)
             .ObserveOn(RxApp.MainThreadScheduler)
             .SubscribeOn(RxApp.TaskpoolScheduler)
             .Subscribe(zz =>
            {
                this.Init(zz.Item2, zz.Item1);

                var actionsAccessed = this.WhenAnyValue(x => x.ActionsAccessed).Where(x => x).Take(1);

                actionsAccessed.SelectMany(_ =>
                {
                    return Observable.CombineLatest(
                        this.WhenAnyValue(y => y.WateringSchedule).Where(y => y != null),
                        this.Actions.ItemsAdded.StartWith(this.Actions).Where(x => x.ActionType == PlantActionType.WATERED),
                        (x, y) => Tuple.Create(x, y)
                     );
                }).Subscribe(x =>
                {
                    if (this.WateringScheduler == null || this.WateringScheduler.Schedule != x.Item1)
                    {
                        this.WateringScheduler = new PlantScheduler(x.Item1, HasWriteAccess) { Icon = IconType.WATER };

                        this.WateringScheduler.WhenAnyValue(y => y.MissedLateAndOwn)
                        .Subscribe(z =>
                        {
                            ShowTileNotification = z && IsWateringScheduleEnabled;
                        });

                    }
                    this.WateringScheduler.LastActionTime = x.Item2.Created;
                });

                actionsAccessed.SelectMany(_ =>
                {
                    return Observable.CombineLatest(
                        this.WhenAnyValue(y => y.FertilizingSchedule).Where(y => y != null),
                        this.Actions.ItemsAdded.StartWith(this.Actions).Where(x => x.ActionType == PlantActionType.FERTILIZED),
                        (x, y) => Tuple.Create(x, y)
                     );
                }).Subscribe(x =>
                {
                    if (this.FertilizingScheduler == null || this.FertilizingScheduler.Schedule != x.Item1)
                        this.FertilizingScheduler = new PlantScheduler(x.Item1, HasWriteAccess) { Icon = IconType.FERTILIZE };
                    this.FertilizingScheduler.LastActionTime = x.Item2.Created;
                });

                actionsAccessed.Subscribe(_ =>
                {
                    SubscribeForActionRemovedSchedulerUpdates();
                });

                this.WhenAnyValue(x => x.FertilizingScheduler.Missed, x => x.WateringScheduler.Missed, (a, b) => a + b)
                .Subscribe(x =>
                {
                    this.MissedCount = (int?)x;
                });

                this.WhenAnyValue(x => x.HasTile).Subscribe(x =>
                {
                    if (HasWriteAccess)
                    {
                        AppBarMenuItems = OwnerMenuItems;
                    }
                });

                this.WhenAnyValue(x => x.IsWateringScheduleEnabled, x => x.WateringScheduler, (x, y) => x && y != null)
                    .ToProperty(this, x => x.ShowWateringScheduler, out _ShowWateringScheduler);

                this.WhenAnyValue(x => x.IsFertilizingScheduleEnabled, x => x.FertilizingScheduler, (x, y) => x && y != null)
                    .ToProperty(this, x => x.ShowFertilizingScheduler, out _ShowFertilizingScheduler);

            });

            DifferentUsersPlantSelected = App.WhenAnyValue(x => x.SelectedPlant).Where(x => x != null && x.UserId != this.UserId);
        }


        public IObservable<IPlantViewModel> DifferentUsersPlantSelected { get; set; }

        // subscribe for updates regarding removed actions
        //
        private void SubscribeForActionRemovedSchedulerUpdates()
        {
            Actions.ItemsRemoved.Where(r => r.ActionType == PlantActionType.WATERED).Subscribe(r =>
            {
                var action = GetLatestAction(PlantActionType.WATERED);
                if (this.WateringScheduler != null)
                {
                    if (action != null)
                    {
                        this.WateringScheduler.LastActionTime = action.Created;
                    }
                    else
                    {
                        this.WateringScheduler = null;
                    }
                }
            });
            Actions.ItemsRemoved.Where(r => r.ActionType == PlantActionType.FERTILIZED).Subscribe(r =>
            {
                var action = GetLatestAction(PlantActionType.WATERED);
                if (this.FertilizingScheduler != null)
                {
                    if (action != null)
                    {
                        this.FertilizingScheduler.LastActionTime = action.Created;
                    }
                    else
                    {
                        this.FertilizingScheduler = null;
                    }
                }
            });
        }


        private IPlantActionViewModel GetLatestAction(PlantActionType type)
        {
            var ret = Actions.Where(x => x.ActionType == type).OrderBy(x => x.ActionIndex).Take(1);
            if (ret.Count() == 0)
            {
                return null;
            }
            return ret.First();
        }



        private GSLocation _Location;
        public GSLocation Location
        {
            get
            {
                return _Location;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Location, value);
            }
        }


        private void Init(Tuple<PlantState, ScheduleState, ScheduleState> stateTuple, IAuthUser appUser)
        {

            this.Log().Info("Init begins");

            var state = stateTuple.Item1;

            this.State = state;
            this.Id = state.Id;
            this.UserId = state.UserId;
            this.IsShared = state.Public;
            this.WateringSchedule = stateTuple.Item2 != null ? new ScheduleViewModel(stateTuple.Item2, ScheduleType.WATERING, App) : null;
            this.FertilizingSchedule = stateTuple.Item3 != null ? new ScheduleViewModel(stateTuple.Item3, ScheduleType.FERTILIZING, App) : null;
            this.IsFertilizingScheduleEnabled = state.IsFertilizingScheduleEnabled;
            this.IsWateringScheduleEnabled = state.IsWateringScheduleEnabled;

            this.HasWriteAccess = state.UserId == appUser.Id;

            AppBarButtons = HasWriteAccess ? GetOwnerButtons() : GetFollowerButtons();
            if (HasWriteAccess)
                AppBarMenuItems = OwnerMenuItems;
            AppBarIsVisible = HasWriteAccess;

            this.ListenTo<NameSet>(this.State.Id).Select(x => x.Name)
                .StartWith(state.Name)
                .Subscribe(x =>
                {
                    this.Name = x;
                });

            this.ListenTo<SpeciesSet>(this.State.Id).Select(x => x.Species)
                .StartWith(state.Species)
                .Subscribe(x => this.Species = x);

            this.WhenAnyValue(x => x.ProfilePictureActionId).Subscribe(x => HandleNewProfilePictureActionId(x));

            ProfilePictureActionId = this.State.ProfilepictureActionId;
            this.ListenTo<ProfilepictureSet>(this.State.Id)
                .Select(x => x.PlantActionId)
                .Subscribe(x =>
                {
                    this.ProfilePictureActionId = x;
                });

            this.ListenTo<MarkedPlantPublic>(this.State.Id)
                .Subscribe(x => this.IsShared = true);

            this.ListenTo<MarkedPlantPrivate>(this.State.Id)
                .Subscribe(x => this.IsShared = false);

            this.ListenTo<ScheduleToggled>(this.State.Id)
                .Subscribe(x =>
                {
                    if (x.Type == ScheduleType.WATERING)
                        this.IsWateringScheduleEnabled = x.IsEnabled;
                    else
                        this.IsFertilizingScheduleEnabled = x.IsEnabled;
                });

            this.ListenTo<TagsSet>(this.State.Id).Select(x => (IList<string>)x.Tags.ToList())
                .StartWith(state.Tags)
                .Subscribe(x => this.Tags = new ReactiveList<string>(x));

            this.ListenTo<LocationSet>(this.State.Id)
                .Select(x => x.Location)
                .StartWith(state.Location)
                .Subscribe(x =>
            {
                Location = x;
            });

            this.App.FutureSchedules(state.Id)
            .Subscribe(x =>
            {
                if (x.Type == ScheduleType.WATERING)
                    this.WateringSchedule = x;
                else
                    this.FertilizingSchedule = x;
            });

            this.WhenAnyValue(x => x.Loaded).Subscribe(_ => UpdateShowPlaceHolder());
            this.WhenAnyValue(x => x.Photo).Subscribe(x =>
            {
                if (x != null)
                {
                    Loaded = true; // having found a profile picture is enough "loaded"
                }
                UpdateShowPlaceHolder();
            });

            this.WhenAnyValue(x => x.ActionsAccessed).Where(x => x).Subscribe(__ =>
            {

                var current = App.CurrentPlantActions(this.State.Id)
                    .ObserveOn(RxApp.MainThreadScheduler)

                    .Do(
                    a => { },

                    () =>
                    {
                        // if after loading finished profile photo uri is null, it means that
                        // profile photo was a photo that was pulled from server and did not 
                        // have a blobkey set, we will try to handle this
                        if (this.Photo != null && this.Photo.Uri == null)
                        {
                            this.Log().Info("current profile picture does not have an URI");
                            if (HasWriteAccess)
                            {
                                TryAssignNewProfilePhoto(null);
                            }
                            else
                            {
                                this.ProfilePictureActionId = null;
                            }
                        }
                        this.Loaded = true;
                    })

                    .Subscribe(a =>
                    {
                        HandleAction(a);
                    });


                var actionsPipe = App.FuturePlantActions(this.State.Id);
                //var actionsPipe = current.Concat(App.FuturePlantActions(this.State.Id));
                actionsPipe.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
                {
                    HandleAction(x);
                });

            });

            var emptyWatering = CreateEmptyActionVM(PlantActionType.WATERED);
            this.WateringCommand.Subscribe(_ => emptyWatering.AddCommand.Execute(null));


            if (HasWriteAccess)
            {

                TryShareCommand
                    .Where(_ => App.EnsureDataConnection())
                    .Select(_ =>
                    {

                        // [1] Check that user is registered and direct to registration if not

                        if (appUser.IsRegistered)
                        {

                            return Observable.Return(new Unit());
                        }
                        else
                        {
                            // register first
                            var svm = new SignInRegisterViewModel(App)
                            {
                                SignInMode = false
                            };
                            this.Navigate(svm);
                            // continue after successfull registration
                            return svm.Response
                                .Where(x => SignInRegisterViewModel.IsSuccess(x))
                                .Take(1)
                                .Do(__ =>
                                {
                                    if (App.Router.NavigationStack.Count > 0)
                                        this.NavigateBack();
                                })
                                .Select(__ => new Unit());

                        }

                    })
                    .Switch()
                    .Select(async _ =>
                    {
                        // [2] Check that the plant is set to shared and prompt if not
                        if (state.Public)
                        {
                            return Observable.Return(new Unit());
                        }
                        else
                        {
                            // had to disable this popup, as it breaks down
                            // in landscape mode -- JOJ 29.1.2014

                            //var popup = this.PlantNotPublicPrompt();
                            //App.ShowPopup.Execute(popup);
                            //return popup.AcceptedObservable
                            //    .Take(1)
                            //    .SelectMany(async __ =>
                            //    {
                            //        await App.HandleCommand(new MarkPlantPublic(state.Id));
                            //        return new Unit();
                            //    }).Take(1);

                            await App.HandleCommand(new MarkPlantPublic(state.Id));
                            return Observable.Return(new Unit());
                        }
                    })
                    .Switch()
                    .Do(x =>
                        {
                            var pvm = new ProgressPopupViewModel()
                            {
                                Caption = "Preparing for sharing",
                                ProgressMessage = "Growth Stories is preparing your plant " + this.Name.ToUpper() + " for sharing" // [SIC]
                            };

                            App.ShowPopup.Execute(pvm);
                        })
                    .SelectMany(async x => await App.Synchronize())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ =>
                    {
                        App.ShowPopup.Execute(null);
                        this.ShareCommand.Execute(null);
                    });
            }

            // kludge:
            //
            // needs to be done to make sure that
            // this.Photo and isLoaded gets set
            // 
            //
            var tmp = Actions;
        
        }


        private IDisposable ppActionsSubs = Disposable.Empty;


        private void HandleNewProfilePictureActionId(Guid? PPActionId)
        {
            ppActionsSubs.Dispose();
            ProfilePictureActionId = PPActionId;

            //this.Log().Info("new profile picture actionid {0}", ProfilePictureActionId);

            if (ProfilePictureActionId == null || !ProfilePictureActionId.HasValue)
            {
                //this.Log().Info("setting profile photo to null", ProfilePictureActionId);
                this.Photo = null;
                return;
            }

            this.Photo = LookupPhoto((Guid)ProfilePictureActionId);
            //this.Log().Info("setting profile photo to {0} after looking up corresponding action", ProfilePictureActionId);

            // tell actions whether they are now the profile photo
            //foreach (var a in Actions.Where(z => z.ActionType == PlantActionType.PHOTOGRAPHED).AsEnumerable())
            //{
            //    SetIsProfilePhoto((IPlantPhotographViewModel)a, PPActionId);
            //}

            ppActionsSubs = this.Actions.ItemsAdded
                .StartWith(Actions)
                .OfType<IPlantPhotographViewModel>()
                //.Where(x => x.PlantActionId == ProfilePictureActionId)
                .Subscribe(x =>
                {
                    if (x.PlantActionId == ProfilePictureActionId)
                    {
                        //this.Log().Info("setting profile photo to {0} after loading corresponding action", ProfilePictureActionId);
                        this.Photo = x.Photo;
                        this.ProfilePictureAction = x;
                    }
                    SetIsProfilePhoto((IPlantPhotographViewModel)x, PPActionId);
                });
        }

        private IPlantPhotographViewModel _ProfilePictureAction;
        protected IPlantPhotographViewModel ProfilePictureAction
        {
            get
            {
                return _ProfilePictureAction;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ProfilePictureAction, value);
            }
        }

        private Photo LookupPhoto(Guid PlantActionId)
        {
            var action = Actions.OfType<IPlantPhotographViewModel>().Where(x => x.PlantActionId == PlantActionId).FirstOrDefault();
            if (action != null)
            {
                ProfilePictureAction = action;
                return action.Photo;
            }
            return null;
        }


        private void SetupMenuItems()
        {

        }

        private IPopupViewModel PlantNotPublicPrompt()
        {
            return new PopupViewModel()
            {
                Caption = string.Format("{0} will be public", this.Name.ToUpper()),
                //Caption = string.Format("{0} is not yet public", this.Name.Substring(0, 1).ToUpper() + this.Name.Substring(1)),
                Message = "Once shared, a plant can be followed by other users.",
                LeftButtonContent = "OK"
            };
        }



        private bool _ShowPlaceHolder;
        public bool ShowPlaceHolder
        {
            get
            {
                return _ShowPlaceHolder;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ShowPlaceHolder, value);
            }
        }


        private void UpdateShowPlaceHolder()
        {
            ShowPlaceHolder = this.Photo == null && Loaded;
        }


        // Notify the viewmodel that the UI failed to
        // download an image
        //
        public void NotifyImageDownloadFailed()
        {
            if (!HasWriteAccess)
            {
                App.NotifyImageDownloadFailed();
            }
            else
            {
                this.Log().Warn("image open failed for non-followed user");
            }
        }


        private void HandleAction(IPlantActionViewModel x)
        {
            PrepareActionVM(x);
            _Actions.Insert(0, x);

            foreach (var a in Actions)
            {
                a.ActionIndex++;
            }

            x.AddCommand.Subscribe(_ => this.PlantActionEdited.Execute(x));

            x.DeleteCommand.Subscribe(_ =>
            {
                HandlePossibleProfilePhotoRemove(x as IPlantPhotographViewModel);
            });


            this.ListenTo<AggregateDeleted>(x.PlantActionId)
                .Subscribe(y =>
            {
                _Actions.Remove(x);

                // this was not right, as the logic for handling removed profile
                // photos should only be triggered when something has been removed
                // from the UI by the user, not for example when running events pulled
                // from server when signing in (*)
                // HandlePossibleProfilePhotoRemove(x as IPlantPhotographViewModel);
            });

            var photo = x as IPlantPhotographViewModel;
            if (photo != null)
            {
                // essentially same comment as above (*)
                // PossiblySetAsProfilePhoto(photo);

                photo.PhotoTimelineTap
                .Subscribe(_ =>
                {
                    var derived = FilteredActions.CreateDerivedCollection(u => (IPlantPhotographViewModel)u, u => u.ActionType == PlantActionType.PHOTOGRAPHED);
                    this.Navigate(new PhotoListViewModel(derived, App, photo));
                });
            }

            ScrollCommand.Execute(x);
        }



        protected IScheduleViewModel _WateringSchedule;
        public IScheduleViewModel WateringSchedule
        {
            get
            {
                return _WateringSchedule;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _WateringSchedule, value);
            }
        }


        protected IScheduleViewModel _FertilizingSchedule;
        public IScheduleViewModel FertilizingSchedule
        {
            get
            {
                return _FertilizingSchedule;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _FertilizingSchedule, value);
            }
        }


        private bool _ActionsAccessed;
        public bool ActionsAccessed
        {
            get
            {
                return _ActionsAccessed;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ActionsAccessed, value);
            }
        }


        protected IReactiveList<string> _Tags;
        public IReactiveList<string> Tags
        {
            get { return _Tags; }
            set { this.RaiseAndSetIfChanged(ref _Tags, value); }
        }

        protected PlantScheduler _WateringScheduler;
        public PlantScheduler WateringScheduler
        {
            get
            {
                return _WateringScheduler;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _WateringScheduler, value);
            }
        }

        protected PlantScheduler _FertilizingScheduler;
        public PlantScheduler FertilizingScheduler
        {
            get
            {
                return _FertilizingScheduler;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _FertilizingScheduler, value);
            }
        }


        private ObservableAsPropertyHelper<bool> _ShowFertilizingScheduler;
        public bool ShowFertilizingScheduler
        {
            get
            {
                return _ShowFertilizingScheduler.Value;
            }
        }

        private ObservableAsPropertyHelper<bool> _ShowWateringScheduler;
        public bool ShowWateringScheduler
        {
            get
            {
                return _ShowWateringScheduler.Value;
            }
        }


        public string TodayWeekDay { get { return SharedViewHelpers.FormatWeekDay(DateTimeOffset.Now); } }
        public string TodayDate { get { return DateTimeOffset.Now.ToString("d"); } }

        protected string _Name;
        public string Name { get { return _Name; } protected set { this.RaiseAndSetIfChanged(ref _Name, value); } }


        private bool _Loaded;
        public bool Loaded
        {
            get
            {
                return _Loaded;
            }
            set
            {
                this.Log().Info("plant {0} loaded is now {1}", this.Id, value);
                this.RaiseAndSetIfChanged(ref _Loaded, value);
            }
        }


        protected Photo _Photo;
        public Photo Photo
        {
            get
            {
                return _Photo;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Photo, value);
            }
        }

        private int? _MissedCount;
        public int? MissedCount
        {
            get
            {
                return _MissedCount;
            }

            protected set
            {
                this.RaiseAndSetIfChanged(ref _MissedCount, value);
            }
        }

        private bool _HasTile;
        public bool HasTile
        {
            get
            {
                return _HasTile;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _HasTile, value);
            }
        }



        protected string _Species;
        public string Species { get { return _Species; } protected set { this.RaiseAndSetIfChanged(ref _Species, value); } }



        private IYAxisShitViewModel _Chart;
        public IYAxisShitViewModel Chart
        {
            get
            {
                return _Chart ?? (_Chart = App.YAxisShitViewModelFactory(this));
            }

        }


        private void PrepareActionVM(IPlantActionViewModel vm)
        {
            vm.PlantId = this.Id;
            vm.UserId = this.UserId;

            // ???? we want the user id of the user who owns the plant/action, what was this?
            //vm.UserId = AppUser != null ? AppUser.Id : default(Guid);
            vm.ActionIndex = 0;
            vm.OwnAction = HasWriteAccess;

            var ma = vm as IPlantMeasureViewModel;
            if (ma != null)
            {
                var list = Actions.CreateDerivedCollection(u => u as IPlantMeasureViewModel, z => z.ActionType == PlantActionType.MEASURED);
                ma.MeasurementActions = list;
            }

            var pa = vm as IPlantPhotographViewModel;
            if (pa != null)
            {
                pa.ActionAddedCommand.OfType<Guid>().Subscribe(guid =>
                {
                    PossiblySetAsProfilePhoto(guid);
                });
            }
        }


        private Guid? _ProfilePictureActionId;
        public Guid? ProfilePictureActionId
        {
            get
            {
                return _ProfilePictureActionId;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ProfilePictureActionId, value);
            }
        }


        private void SetIsProfilePhoto(IPlantPhotographViewModel pa, Guid? PPActionId)
        {
            //this.Log().Info("set is profile photo {0} {1}", pa.PlantActionId, PPActionId);
            if (pa != null)
            {
                pa.IsProfilePhoto = PPActionId == pa.PlantActionId;
            }

            //if (pa != null && Photo != null && pa.State != null && pa.State.Photo != null)
            //{
            //    // hack, works for now
            //    pa.IsProfilePhoto =
            //        (Photo.LocalUri != null && Photo.LocalUri == pa.State.Photo.LocalUri)
            //        || (Photo.RemoteUri != null && Photo.RemoteUri == pa.State.Photo.RemoteUri);
            //}
            //else if (pa != null)
            //{
            //    pa.IsProfilePhoto = false;
            //}
        }


        private IPlantActionViewModel CreateEmptyActionVM(PlantActionType type)
        {
            var vm = App.PlantActionViewModelFactory(type);
            PrepareActionVM(vm);

            return vm;
        }


        public IReactiveCommand _NavigateToEmptyActionCommand;
        public IReactiveCommand NavigateToEmptyActionCommand
        {

            get
            {
                if (_NavigateToEmptyActionCommand == null)
                {
                    _NavigateToEmptyActionCommand = new ReactiveCommand();
                    _NavigateToEmptyActionCommand.OfType<PlantActionType>().Subscribe(x =>
                    {
                        var vm = CreateEmptyActionVM(x);

                        vm.AddCommand.Take(1).Subscribe(_ => App.Router.NavigateBack.Execute(null));

                        this.Navigate(vm);
                    });
                    // the request came from the action picker
                    _NavigateToEmptyActionCommand.OfType<Tuple<PlantActionType, string>>()
                        .Where(x => x.Item2 == PlantActionListViewModel.ACTIONLIST_ID).Subscribe(x =>
                    {
                        var vm = CreateEmptyActionVM(x.Item1);
                        // remove one from the stack
                        var stack = App.Router.NavigationStack;
                        stack.RemoveAt(stack.Count - 1);

                        vm.AddCommand.Take(1).Subscribe(_ => App.Router.NavigateBack.Execute(null));

                        this.Navigate(vm);
                    });
                }
                return _NavigateToEmptyActionCommand;
            }

        }

        public IReactiveCommand _PlantActionEdited;
        public IReactiveCommand PlantActionEdited
        {

            get
            {
                if (_PlantActionEdited == null)
                {
                    _PlantActionEdited = new ReactiveCommand();
                    _PlantActionEdited.OfType<IPlantActionViewModel>().Subscribe(x => App.Router.NavigateBack.Execute(null));
                }
                return _PlantActionEdited;
            }

        }


        public int PlantIndex { get; set; }


        protected ReactiveList<IPlantActionViewModel> _Actions;
        public IReadOnlyReactiveList<IPlantActionViewModel> Actions
        {
            get
            {
                if (_Actions == null)
                {
                    _Actions = new ReactiveList<IPlantActionViewModel>();
                    this.ActionsAccessed = true;
                }

                return _Actions;
            }
        }



        public IReactiveDerivedList<IPlantActionViewModel> _FilteredActions;
        public IReactiveDerivedList<IPlantActionViewModel> FilteredActions
        {
            get
            {
                if (_FilteredActions == null)
                {
                    _FilteredActions = Actions
                        .CreateDerivedCollection(x => x, x => x.ActionType != PlantActionType.PHOTOGRAPHED
                            || (x.Photo != null && x.Photo.Uri != null));
                }
                return _FilteredActions;
            }
        }


        private IPlantPhotographViewModel LatestPhoto(IPlantPhotographViewModel excluding)
        {
            var list = Actions
                .Where(x => x.ActionType == PlantActionType.PHOTOGRAPHED && x.Photo != null && x.Photo.Uri != null && x != excluding)
                .OrderByDescending(x => x.Created)
                .Take(1);

            if (list.Count() == 1)
            {
                var action = list.First() as IPlantPhotographViewModel;
                return action;
            }
            return null;
        }


        // Set the photo represented by the given viewmodel as profile photo,
        // if there is no profile photo yet
        //
        // (Not necessarily stricly correct to have this in the ViewModel, but
        //  will do for now)
        private void PossiblySetAsProfilePhoto(Guid plantActionId)
        {
            if (!HasWriteAccess)
            {
                return;
            }
            //this.Log().Info("possiblysetasprofilephoto {0} {1}", plantActionId, ProfilePictureActionId);
            if (ProfilePictureActionId == null || !ProfilePictureActionId.HasValue || Photo == null)
            {
                App.HandleCommand(new SetProfilepicture(this.Id, plantActionId));
            }
        }


        // Set latest photo as profile picture, if the given viewmodel represents
        // a profile photo and any photos are left for this plant
        //
        // (Not necessarily stricly correct to have this in the ViewModel, but
        //  will do for now)
        private void HandlePossibleProfilePhotoRemove(IPlantPhotographViewModel vm)
        {
            if (vm == null || !HasWriteAccess)
            {
                return;
            }

            if (vm.IsProfilePhoto)
            {
                TryAssignNewProfilePhoto(vm);
            }
        }


        private void TryAssignNewProfilePhoto(IPlantPhotographViewModel previous)
        {
            if (!HasWriteAccess)
            {
                return;
            }

            var latest = LatestPhoto(previous);
            if (latest != null)
            {
                App.HandleCommand(new SetProfilepicture((Guid)latest.PlantId, latest.PlantActionId));
            }
            else
            {
                //this.Log().Info("after remove, profile picture actionId is null");
                this.ProfilePictureActionId = null;
            }

        }


        private bool _ShowTileNotification;
        public bool ShowTileNotification
        {
            get
            {
                return _ShowTileNotification;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _ShowTileNotification, value);
            }
        }


        public override string UrlPathSegment
        {
            get { return string.Format("type=plant&id={0}", this.Id); }
        }

        #region APPBAR
        private IReadOnlyReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _AppBarButtons, value);
            }
        }



        private ReactiveList<IButtonViewModel> GetFollowerButtons()
        {
            return new ReactiveList<IButtonViewModel>();
        }


        private ReactiveList<IButtonViewModel> GetOwnerButtons()
        {
            return new ReactiveList<IButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "water",
                            IconType = IconType.WATER,
                            Command = this.WateringCommand,
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "photograph",
                            IconType = IconType.PHOTO,
                            Command = this.PhotoCommand
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "comment",
                            IconType = IconType.NOTE,
                            Command = NavigateToEmptyActionCommand,
                            CommandParameter = PlantActionType.COMMENTED
                        },
                        new ButtonViewModel(null)
                        {
                            Text = "share",
                            IconType = IconType.SHARE,
                            Command = TryShareCommand
                        },

                    };
        }






        private ReactiveList<IMenuItemViewModel> OwnerMenuItems
        {
            get
            {
                ReactiveList<IMenuItemViewModel> ret = new ReactiveList<IMenuItemViewModel>()
                    {
                        new MenuItemViewModel(null)
                        {
                            Text = "pick action",
                            Command = this.ShowActionList   
                        },                
                        new MenuItemViewModel(null)
                        {
                            Text = "edit",
                            Command = EditCommand,
                        },                                              
                        new MenuItemViewModel(null)
                        {
                            Text = "delete",
                            Command = DeleteRequestedCommand,
                            CommandParameter = DeleteRequestOrigin.SELF
                        },
                        new MenuItemViewModel(null)
                            {
                                Text = HasTile ? "unpin" : "pin",
                                Command = PinCommand
                            }
                                        
                        };


                return ret;
            }
        }



        protected IReadOnlyReactiveList<IMenuItemViewModel> _AppBarMenuItems;
        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
        {
            get
            {

                return _AppBarMenuItems;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _AppBarMenuItems, value);
            }
        }

        public ApplicationBarMode AppBarMode { get { return ApplicationBarMode.DEFAULT; } }
        private bool _AppBarIsVisible = true;
        public bool AppBarIsVisible
        {
            get
            {
                return _AppBarIsVisible;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _AppBarIsVisible, value);
            }
        }


        #endregion

        public SupportedPageOrientation SupportedOrientations
        {
            get { return SupportedPageOrientation.PortraitOrLandscape; }
        }



        public IPopupViewModel DeleteConfirmation()
        {
            return new PopupViewModel()
            {
                Caption = "Confirm delete",
                Message = "Are you sure you wish to delete the plant "
                        + Name.ToUpper()
                        + "? This can't be undone.",

                IsLeftButtonEnabled = true,
                IsRightButtonEnabled = true,
                LeftButtonContent = "Yes",
                RightButtonContent = "Cancel"
            };
        }

    }


    public class EmptyPlantViewModel : IPlantViewModel
    {

        public bool Loaded { get; set; }

        public bool ShouldBeFullyLoaded { get; set; }

        public bool ShowPlaceHolder { get; set; }

        public GSLocation Location { get; set; }

        public IObservable<IPlantViewModel> DifferentUsersPlantSelected { get; set; }


        public void NotifyImageDownloadFailed()
        {

        }

        private static EmptyPlantViewModel _Instance;

        public static EmptyPlantViewModel Instance
        {
            get
            {
                return _Instance ?? (_Instance = new EmptyPlantViewModel());
            }
        }

        public bool HasWriteAccess { get; set; }

        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string Species { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public IReactiveCommand PinCommand { get; set; }

        public IReactiveCommand ShareCommand { get; set; }

        public IReactiveCommand ScrollCommand { get; set; }

        public IReactiveCommand WateringCommand { get; set; }

        public IReactiveCommand ShowDetailsCommand { get; set; }

        public IObservable<IPlantViewModel> WateringObservable { get; set; }

        public IReactiveCommand DeleteCommand { get; set; }

        public IReactiveCommand NavigateToEmptyActionCommand { get; set; }

        public IReactiveCommand ShowActionList { get; set; }

        public IReactiveCommand ResetAnimationsCommand { get; set; }

        public IReactiveList<string> Tags { get; set; }

        public Photo Photo { get; set; }

        public int? MissedCount { get; set; }

        public bool HasTile { get; set; }

        public bool IsShared { get; set; }

        public bool IsFertilizingScheduleEnabled { get; set; }

        public bool IsWateringScheduleEnabled { get; set; }

        public IReadOnlyReactiveList<IPlantActionViewModel> Actions { get; set; }

        public IReactiveDerivedList<IPlantActionViewModel> FilteredActions { get; set; }

        public IPlantActionViewModel SelectedItem { get; set; }

        public IScheduleViewModel WateringSchedule { get; set; }

        public IScheduleViewModel FertilizingSchedule { get; set; }

        public IYAxisShitViewModel Chart { get; set; }

        public PlantScheduler WateringScheduler { get; set; }

        public PlantScheduler FertilizingScheduler { get; set; }

        public string TodayWeekDay { get; set; }

        public string TodayDate { get; set; }

        public int PlantIndex { get; set; }

        public string UrlPath { get; set; }

        public string UrlPathSegment { get; set; }

        public IScreen HostScreen { get; set; }

        public IObservable<IObservedChange<object, object>> Changing { get; set; }

        public IObservable<IObservedChange<object, object>> Changed { get; set; }

        public IDisposable SuppressChangeNotifications()
        {
            return Disposable.Empty;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons { get; set; }

        public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems { get; set; }

        public ApplicationBarMode AppBarMode { get; set; }

        public bool AppBarIsVisible { get; set; }

        public SupportedPageOrientation SupportedOrientations { get; set; }

        public IObservable<IPlantViewModel> DeleteObservable { get; set; }

        public IReactiveCommand DeleteRequestedCommand { get; set; }

        public IReactiveCommand PhotoCommand { get; set; }

        public IReactiveCommand TryShareCommand { get; set; }

    }


}