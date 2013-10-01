
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System;
using System.Collections.Generic;
using Ninject;
using Growthstories.Domain.Entities;
using Growthstories.Core;
using Growthstories.Sync;
using Growthstories.Domain.Messaging;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{

    public interface IGSApp : IGSRoutableViewModel, IScreen, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
    {
        bool IsInDesignMode { get; }
        string AppName { get; }
        IMessageBus Bus { get; }
        IUserService Context { get; }
        IDictionary<IconType, Uri> IconUri { get; }
        IDictionary<IconType, Uri> BigIconUri { get; }
        IMutableDependencyResolver Resolver { get; }

        Task<SyncResult> Synchronize();

        IPlantActionViewModel PlantActionViewModelFactory<T>(PlantActionState state = null) where T : IPlantActionViewModel;
        IObservable<IPlantActionViewModel> PlantActionViewModelFactory(PlantState state, Guid? PlantActionId = null);

        ScheduleViewModel ScheduleViewModelFactory(PlantState plantState, ScheduleType scheduleType);
        AddPlantViewModel AddPlantViewModelFactory(PlantState state);

        PageOrientation Orientation { get; }
        Task AddTestData();
        Task ClearDB();

    }


    public class AppViewModel : ReactiveObject, IGSApp
    {

        protected ObservableAsPropertyHelper<SupportedPageOrientation> _SupportedOrientations;
        public SupportedPageOrientation SupportedOrientations
        {
            get
            {
                return _SupportedOrientations.Value;
            }
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

            var resolver = RxApp.MutableResolver;
            this.Resolver = resolver;

            resolver.RegisterConstant(this, typeof(IScreen));
            resolver.RegisterConstant(this.Router, typeof(IRoutingState));


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

            resolver.RegisterLazySingleton(() => new MainViewModel(this.GardenFactory, this), typeof(IMainViewModel));
            resolver.RegisterLazySingleton(() => new NotificationsViewModel(this), typeof(INotificationsViewModel));
            resolver.RegisterLazySingleton(() => new FriendsViewModel(this), typeof(IFriendsViewModel));
            //resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));




        }

        protected UserState _CurrentUserState;
        public UserState CurrentUserState
        {
            get
            {
                if (_CurrentUserState == null)
                {
                    _CurrentUserState = Context.CurrentUser as UserState;
                }
                return _CurrentUserState;
            }
        }

        public IGardenViewModel GardenFactory(Guid id)
        {
            return new GardenViewModel(
                CurrentUserState.Gardens[id],
                this.PlantFactory,
                this
            );

        }

        public IPlantViewModel PlantFactory(Guid id, IGardenViewModel garden)
        {
            return new PlantViewModel(
             ((Plant)Kernel.Get<IGSRepository>().GetById(id)).State,
             garden,
             this
            );

        }

        protected ObservableAsPropertyHelper<bool> _AppBarIsVisible;
        public bool AppBarIsVisible
        {
            get { return _AppBarIsVisible.Value; }
        }

        protected ObservableAsPropertyHelper<ApplicationBarMode> _AppBarMode;
        public ApplicationBarMode AppBarMode
        {
            get { return _AppBarMode.Value; }
        }

        private void UpdateAppBar(IList<ButtonViewModel> x)
        {
            this.AppBarButtons.RemoveRange(0, this.AppBarButtons.Count);
            this.AppBarButtons.AddRange(x);
        }

        private void UpdateMenuItems(IList<MenuItemViewModel> x)
        {
            this.AppBarMenuItems.RemoveRange(0, this.AppBarMenuItems.Count);
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



        IUserService _Context;
        public IUserService Context
        {
            get { return _Context ?? (_Context = Kernel.Get<IUserService>()); }
        }


        ISynchronizerService _SyncService;
        public Task<SyncResult> Synchronize()
        {
            if (_SyncService == null)
                _SyncService = Kernel.Get<ISynchronizerService>();

            return _SyncService.Synchronize();
        }

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

        public virtual IObservable<IPlantActionViewModel> PlantActionViewModelFactory(PlantState state, Guid? PlantActionId = null)
        {


            Func<Guid?, Guid?, Guid?, IEnumerable<PlantActionState>> f = Kernel.Get<IUIPersistence>().GetActions;

            var af = f.ToAsync(RxApp.InUnitTestRunner() ? RxApp.MainThreadScheduler : RxApp.TaskpoolScheduler);

            return af(PlantActionId, state.Id, state.UserId)
                //.OfType<User>()
                .Select(x => x.ToObservable())
                .Switch()
                .Select(x => PlantActionViewModelFactory<IPlantActionViewModel>(x));



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

