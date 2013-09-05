
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{


    public class GardenViewModel : RoutableViewModel, IPanoramaPage
    {

        private ReactiveList<PlantStateViewModel> _Plants;
        public ReactiveList<PlantStateViewModel> Plants
        {
            get
            {
                if (_Plants == null)
                {
                    if (IsInDesignMode)
                        Plants = new ReactiveList<PlantStateViewModel>(this.PlantProjection.FakeLoadWithUserId(this.UserId).Select(x => new PlantStateViewModel(x)));
                    else
                        Plants = new ReactiveList<PlantStateViewModel>(this.PlantProjection.LoadWithUserId(this.UserId).Select(x => new PlantStateViewModel(x)));

                }
                return _Plants;
            }
            private set
            {
                if (_Plants == value)
                    return;
                _Plants = value;
                _Plants.CollectionChanged += (a, aa) => this.RefreshButtons();
            }
        }

        private IList<PlantStateViewModel> _SelectedPlants;
        public IList<PlantStateViewModel> SelectedPlants
        {
            get
            {
                return _SelectedPlants == null ? _SelectedPlants = new List<PlantStateViewModel>() : _SelectedPlants;
            }
            private set
            {
                if (_SelectedPlants == value)
                    return;
                _SelectedPlants = value;
            }
        }
        public event EventHandler ButtonsRefreshed;
        public PlantProjection PlantProjection { get; private set; }
        private readonly Guid UserId;



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GardenViewModel(
            Guid userId,
             PlantProjection plantProjection,
            IUserService ctx,
            IMessageBus handler,
            INavigationService nav)

            : base(ctx, handler, nav)
        {
            this.UserId = userId;
            this.PlantProjection = plantProjection;
            this.PlantProjection.EventHandled += this.EventHandled;
            this.PropertyChanged += (a, aa) => this.RefreshButtons();

        }

        private void RefreshButtons()
        {
            this.AppBarButtons.Clear();
            if (this.IsPlantSelectionEnabled)
            {
                this.AppBarButtons.Add(this.DeletePlantsButton);
            }
            else
            {
                this.AppBarButtons.Add(this.AddPlantButton);
                if (this.Plants.Count > 0)
                {
                    this.AppBarButtons.Add(this.SelectPlantsButton);
                    this.AppBarButtons.Add(this.ChangePPicButton);
                }
            }
            this.ButtonsRefreshed(this, new EventArgs());

        }


        private void EventHandled(object sender, EventHandledArgs e)
        {
            var @event = e.@event as PlantCreated;
            if (@event != null && @event.UserId == this.UserId)
            {
                if (!this.Plants.Any(x => x.Id == @event.EntityId))
                    this.Plants.Add(new PlantStateViewModel(this.PlantProjection.LoadWithId(@event.EntityId)));
            }
        }




        #region AddPlant


        private ReactiveCommand _ShowAddPlantCommand;
        public ReactiveCommand ShowAddPlantCommand
        {
            get
            {

                if (_ShowAddPlantCommand == null)
                {
                    _ShowAddPlantCommand = new ReactiveCommand();
                    _ShowAddPlantCommand.Subscribe(_ => Nav.NavigateTo(View.ADD_PLANT));
                }
                return _ShowAddPlantCommand;

            }
        }

        protected bool _IsPlantSelectionEnabled = false;
        public bool IsPlantSelectionEnabled
        {
            get
            {
                return _IsPlantSelectionEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsPlantSelectionEnabled, value);
                //this.RefreshButtons();
            }
        }


        //private ReactiveCommand _PlantSelectionToggleCommand;
        //public ReactiveCommand PlantSelectionToggleCommand
        //{
        //    get
        //    {

        //        if (_PlantSelectionToggleCommand == null)
        //            _PlantSelectionToggleCommand = new ReactiveCommand(() =>
        //            {
        //                this.
        //            });
        //        return _PlantSelectionToggleCommand;

        //    }
        //}


        #endregion

        private PlantStateViewModel _SelectedPlant;
        public PlantStateViewModel SelectedPlant
        {
            get { return _SelectedPlant; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _SelectedPlant, value);
            }
        }
        //public event EventHandler<SelectedPlantArgs> PlantSelected;
        private ReactiveCommand _ShowDetailsCommand;
        public ReactiveCommand ShowDetailsCommand
        {
            get
            {

                if (_ShowDetailsCommand == null)
                {
                    _ShowDetailsCommand = new ReactiveCommand();
                    _ShowDetailsCommand.Subscribe((p) =>
                    {
                        this.SelectedPlant = (PlantStateViewModel)p;
                        //MessengerInstance.Send(new ShowPlantView(p));
                        //Nav.NavigateTo(View.PLANT);
                    });
                }
                return _ShowDetailsCommand;

            }
        }

        private ReactiveCommand _SelectedPlantsChangedCommand;
        public ReactiveCommand SelectedPlantsChangedCommand
        {
            get
            {

                if (_SelectedPlantsChangedCommand == null)
                {
                    _SelectedPlantsChangedCommand = new ReactiveCommand();

                    _SelectedPlantsChangedCommand.Subscribe(p =>
                    {
                        RefreshPlantsSelection((IList)p);
                        //this.SelectedPlant = p;
                        //MessengerInstance.Send(new ShowPlantView(p));
                        //Nav.NavigateTo(View.PLANT);
                    });
                }
                return _SelectedPlantsChangedCommand;

            }
        }

        private void RefreshPlantsSelection(IList selected)
        {
            try
            {
                var s = selected.Cast<PlantStateViewModel>();
                foreach (var p in s.Except(this.SelectedPlants))
                {
                    this.SelectedPlants.Add(p);
                }
                foreach (var p in this.SelectedPlants.Except(s))
                {
                    this.SelectedPlants.Remove(p);
                }
            }
            catch (Exception)
            {

                //throw;
            }
        }



        private ButtonViewModel _AddPlantButton;
        public ButtonViewModel AddPlantButton
        {
            get
            {
                if (_AddPlantButton == null)
                    _AddPlantButton = new ButtonViewModel()
                    {
                        Text = "add",
                        IconUri = Nav.IconUri[IconType.ADD],
                        Command = ShowAddPlantCommand
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
                    _ChangePPicButton = new ButtonViewModel()
                    {
                        Text = "ppic",
                        IconUri = Nav.IconUri[IconType.ADD],
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
                    _SelectPlantsButton = new ButtonViewModel()
                    {
                        Text = "select",
                        IconUri = Nav.IconUri[IconType.CHECK_LIST],
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
                    _DeletePlantsButton = new ButtonViewModel()
                    {
                        Text = "delete",
                        IconUri = Nav.IconUri[IconType.DELETE],
                        //Command = new ReactiveCommand(() => { })
                    };
                return _DeletePlantsButton;
            }
        }

        private ReactiveList<ButtonViewModel> _Buttons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_Buttons == null)
                {
                    _Buttons = new ReactiveList<ButtonViewModel>() { AddPlantButton };
                }
                return _Buttons;
            }
        }


    }
}