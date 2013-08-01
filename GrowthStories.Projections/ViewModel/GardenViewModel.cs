using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{


    public class GardenViewModel : GSViewModelBase, IPanoramaPage
    {

        private ObservableCollection<PlantStateViewModel> _Plants;
        public ObservableCollection<PlantStateViewModel> Plants
        {
            get
            {
                if (_Plants == null)
                {
                    if (IsInDesignMode)
                        Plants = new ObservableCollection<PlantStateViewModel>(this.PlantProjection.FakeLoadWithUserId(this.UserId).Select(x => new PlantStateViewModel(x)));
                    else
                        Plants = new ObservableCollection<PlantStateViewModel>(this.PlantProjection.LoadWithUserId(this.UserId).Select(x => new PlantStateViewModel(x)));

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
            IMessenger messenger,
            IUserService ctx,
            IDispatchCommands handler,
            INavigationService nav)

            : base(messenger, ctx, handler, nav)
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


        private RelayCommand _ShowAddPlantCommand;
        public RelayCommand ShowAddPlantCommand
        {
            get
            {

                if (_ShowAddPlantCommand == null)
                    _ShowAddPlantCommand = new RelayCommand(() =>
                    {
                        Nav.NavigateTo(View.ADD_PLANT);
                    });
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
                Set("IsPlantSelectionEnabled", ref _IsPlantSelectionEnabled, value);
                //this.RefreshButtons();
            }
        }


        //private RelayCommand _PlantSelectionToggleCommand;
        //public RelayCommand PlantSelectionToggleCommand
        //{
        //    get
        //    {

        //        if (_PlantSelectionToggleCommand == null)
        //            _PlantSelectionToggleCommand = new RelayCommand(() =>
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
                Set(() => SelectedPlant, ref _SelectedPlant, value);
            }
        }
        //public event EventHandler<SelectedPlantArgs> PlantSelected;
        private RelayCommand<PlantStateViewModel> _ShowDetailsCommand;
        public RelayCommand<PlantStateViewModel> ShowDetailsCommand
        {
            get
            {

                if (_ShowDetailsCommand == null)
                    _ShowDetailsCommand = new RelayCommand<PlantStateViewModel>(p =>
                    {
                        this.SelectedPlant = p;
                        //MessengerInstance.Send(new ShowPlantView(p));
                        //Nav.NavigateTo(View.PLANT);
                    });
                return _ShowDetailsCommand;

            }
        }

        private RelayCommand<IList> _SelectedPlantsChangedCommand;
        public RelayCommand<IList> SelectedPlantsChangedCommand
        {
            get
            {

                if (_SelectedPlantsChangedCommand == null)
                    _SelectedPlantsChangedCommand = new RelayCommand<IList>(p =>
                    {
                        RefreshPlantsSelection(p);
                        //this.SelectedPlant = p;
                        //MessengerInstance.Send(new ShowPlantView(p));
                        //Nav.NavigateTo(View.PLANT);
                    });
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
                    _ChangePPicButton = new ButtonViewModel()
                    {
                        Text = "ppic",
                        IconUri = Nav.IconUri[IconType.ADD],
                        Command = new RelayCommand(() =>
                        {
                            if (this.Plants.Count > 0)
                            {
                                Handler.Handle(new ChangeProfilepicturePath(this.Plants[0].Id, PlantProjection.testPic1));
                            }
                        })
                    };
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
                        Command = new RelayCommand(() => this.IsPlantSelectionEnabled = true)
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
                        Command = new RelayCommand(() => { })
                    };
                return _DeletePlantsButton;
            }
        }

        private ObservableCollection<ButtonViewModel> _Buttons;
        public ObservableCollection<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_Buttons == null)
                {
                    _Buttons = new ObservableCollection<ButtonViewModel>() { AddPlantButton };
                }
                return _Buttons;
            }
        }
    }
}