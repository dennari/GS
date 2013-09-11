
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{
    public interface IGardenViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public interface INotificationsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public interface IFriendsViewModel : IGSViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public class GardenViewModel : RoutableViewModel, IGardenViewModel
    {

        public ReactiveList<PlantStateViewModel> Plants { get; protected set; }
        public ReactiveList<PlantStateViewModel> SelectedPlants { get; protected set; }
        public ReactiveList<ButtonViewModel> AppBarButtons { get; protected set; }

        public IReactiveCommand SelectedPlantsChangedCommand { get; protected set; }
        public IReactiveCommand ShowDetailsCommand { get; protected set; }



        public PlantProjection PlantProjection { get; private set; }
        private readonly Guid UserId;



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GardenViewModel(
            Guid userId,
            Func<Guid, IPlantViewModel> pvmFactory,
            IUserService ctx,
            IMessageBus bus,
            IScreen host)

            : base(ctx, bus, host)
        {
            this.UserId = userId;

            this.Plants = new ReactiveList<PlantStateViewModel>();
            this.SelectedPlants = new ReactiveList<PlantStateViewModel>();
            this.AppBarButtons = new ReactiveList<ButtonViewModel>();


            this.Plants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(SelectPlantsButton));
            this.Plants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(SelectPlantsButton));

            this.SelectedPlants.IsEmptyChanged.Where(x => x == false).Subscribe(_ => AppBarButtons.Add(DeletePlantsButton));
            this.SelectedPlants.IsEmptyChanged.Where(x => x == true).Subscribe(_ => AppBarButtons.Remove(DeletePlantsButton));


            SelectedPlantsChangedCommand = new ReactiveCommand();
            SelectedPlantsChangedCommand.Subscribe(p =>
            {
                SelectedPlants.Clear();
                SelectedPlants.AddRange(((IList)p).Cast<PlantStateViewModel>());
            });

            ShowDetailsCommand = new ReactiveCommand();
            ShowDetailsCommand.Subscribe(x =>
            {
                host.Router.Navigate.Execute(pvmFactory((Guid)x));
            });

        }


        private ButtonViewModel _AddPlantButton;
        public ButtonViewModel AddPlantButton
        {
            get
            {
                if (_AddPlantButton == null)
                    _AddPlantButton = new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconUri = GSApp.IconUri[IconType.ADD],
                        Command = this.HostScreen.Router.NavigateCommandFor<IAddPlantViewModel>()
                    };
                return _AddPlantButton;
            }
        }

        private ButtonViewModel _ChangePPicButton;
        public ButtonViewModel ChangePPicButton
        {
            get
            {
                if (_ChangePPicButton == null)
                {

                    var Command = new ReactiveCommand();
                    Command.Subscribe(_ =>
                     {
                         if (this.Plants.Count > 0)
                         {
                             Bus.Handle(new ChangeProfilepicturePath(this.Plants[0].Id, PlantProjection.testPic1));
                         }
                     });
                    _ChangePPicButton = new ButtonViewModel(this.Bus)
                    {
                        Text = "ppic",
                        //IconUri = Nav.IconUri[IconType.ADD],
                        Command = Command
                    };
                }
                return _ChangePPicButton;
            }
        }

        private ButtonViewModel _SelectPlantsButton;
        public ButtonViewModel SelectPlantsButton
        {
            get
            {
                if (_SelectPlantsButton == null)
                    _SelectPlantsButton = new ButtonViewModel(this.Bus)
                    {
                        Text = "select",
                        IconUri = GSApp.IconUri[IconType.CHECK_LIST]
                        //Command = new ReactiveCommand(() => this.IsPlantSelectionEnabled = true)
                    };
                return _SelectPlantsButton;
            }
        }

        private ButtonViewModel _DeletePlantsButton;
        public ButtonViewModel DeletePlantsButton
        {
            get
            {
                if (_DeletePlantsButton == null)
                    _DeletePlantsButton = new ButtonViewModel(this.Bus)
                    {
                        Text = "delete",
                        IconUri = GSApp.IconUri[IconType.DELETE]
                        //Command = new ReactiveCommand(() => { })
                    };
                return _DeletePlantsButton;
            }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public string AppBarMode
        {
            get { return GSApp.APPBAR_MODE_MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }
    }

    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {
        public NotificationsViewModel(IUserService ctx, IMessageBus bus, IScreen host)
            : base(ctx, bus, host)
        {

            this.AppBarButtons.Add(
            new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconUri = GSApp.IconUri[IconType.ADD],
                        Command = this.HostScreen.Router.NavigateCommandFor<IAddPlantViewModel>()
                    });

        }
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons ?? (_AppBarButtons = new ReactiveList<ButtonViewModel>()); }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public string AppBarMode
        {
            get { return GSApp.APPBAR_MODE_MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }
    }

    public class FriendsViewModel : RoutableViewModel, IFriendsViewModel
    {
        public FriendsViewModel(IUserService ctx, IMessageBus bus, IScreen host)
            : base(ctx, bus, host)
        {

            this.AppBarButtons.Add(
            new ButtonViewModel(null)
            {
                Text = "add",
                IconUri = GSApp.IconUri[IconType.ADD],
                Command = this.HostScreen.Router.NavigateCommandFor<IAddPlantViewModel>()
            });

        }
        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons ?? (_AppBarButtons = new ReactiveList<ButtonViewModel>()); }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public string AppBarMode
        {
            get { return GSApp.APPBAR_MODE_MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }
    }
}