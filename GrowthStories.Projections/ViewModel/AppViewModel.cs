
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

namespace Growthstories.UI.ViewModel
{

    public interface IGSApp : IGSViewModel, IScreen, IHasAppBarButtons, IControlsAppBar
    {
        bool IsInDesignMode { get; }
        string AppName { get; }
        IMessageBus Bus { get; }
        IUserService Context { get; }
        IDictionary<IconType, Uri> IconUri { get; }
        IMutableDependencyResolver Resolver { get; }
    }


    public class AppViewModel : ReactiveObject, IGSApp
    {

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
                .OfType<IControlsAppBar>()
                .Select(x => x.WhenAny(y => y.AppBarMode, y => y.GetValue()).StartWith(x.AppBarMode))
                .Switch()
                .ToProperty(this, x => x.AppBarMode, out this._AppBarMode, ApplicationBarMode.MINIMIZED);

            this.Router.CurrentViewModel
                 .OfType<IControlsAppBar>()
                 .Select(x => x.WhenAny(y => y.AppBarIsVisible, y => y.GetValue()).StartWith(x.AppBarIsVisible))
                 .Switch()
                 .ToProperty(this, x => x.AppBarIsVisible, out this._AppBarIsVisible, true);



            resolver.RegisterLazySingleton(() => new MainViewModel(this.GardenFactory, this), typeof(IMainViewModel));
            resolver.RegisterLazySingleton(() => new NotificationsViewModel(this), typeof(INotificationsViewModel));
            resolver.RegisterLazySingleton(() => new FriendsViewModel(this), typeof(IFriendsViewModel));
            resolver.RegisterLazySingleton(() => new AddPlantViewModel(this), typeof(IAddPlantViewModel));


        }

        public IGardenViewModel GardenFactory(Guid id)
        {
            return new GardenViewModel(
                ((Garden)Kernel.Get<IGSRepository>().GetById(id)).State,
                this.PlantFactory,
                this
            );

        }

        public IPlantViewModel PlantFactory(Guid id)
        {
            return new PlantViewModel(
                ((Plant)Kernel.Get<IGSRepository>().GetById(id)).State,
                this.ActionFactory,
                this
            );

        }

        public IEnumerable<ActionBase> ActionFactory(object ids)
        {
            //return new PlantViewModel(
            //    ((Plant)Kernel.Get<IGSRepository>().GetById(id)).State,
            //    this
            //);
            var Ids = ids as Tuple<Guid, Guid>;
            if (Ids == null)
            {
                return null;
            }
            var user = ((User)Kernel.Get<IGSRepository>().GetById(Ids.Item1)).State;

            return user.Actions.Where(x => x.PlantId == Ids.Item2);

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


        protected ReactiveList<ButtonViewModel> _AppBarButtons = new ReactiveList<ButtonViewModel>();
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
        }

        IDictionary<IconType, Uri> _IconUri = new Dictionary<IconType, Uri>()
        {
            {IconType.ADD,new Uri("/Assets/Icons/appbar.add.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK,new Uri("/Assets/Icons/appbar.check.png", UriKind.RelativeOrAbsolute)},
            {IconType.DELETE,new Uri("/Assets/Icons/appbar.delete.png", UriKind.RelativeOrAbsolute)},
            {IconType.CHECK_LIST,new Uri("/Assets/Icons/appbar.list.check.png", UriKind.RelativeOrAbsolute)}
        };

        public IDictionary<IconType, Uri> IconUri { get { return _IconUri; } }



        IUserService _Context;
        public IUserService Context
        {
            get { return _Context ?? (_Context = Kernel.Get<IUserService>()); }
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
        CHECK_LIST
    }

    public enum ApplicationBarMode
    {
        DEFAULT,
        MINIMIZED
    }



}

