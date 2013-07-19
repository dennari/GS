using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{


    public class GardenViewModel : GSViewModelBase
    {
        public bool Loaded = false;
        public PlantProjection PlantProjection { get; private set; }
        public ObservableCollection<PlantCreated> Plants { get; private set; }


        protected string _NewPlantName;

        public string NewPlantName
        {
            get
            {
                return _NewPlantName;
            }
            set
            {
                Set(() => NewPlantName, ref _NewPlantName, value);
            }
        }


        protected Guid _NewPlantId;
        public Guid NewPlantId
        {
            get
            {
                return _NewPlantId == default(Guid) ? Guid.NewGuid() : _NewPlantId;
            }
            set
            {
                Set(() => NewPlantId, ref _NewPlantId, value);
            }
        }



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public GardenViewModel(IMessenger messenger, IUserService ctx, IDispatchCommands handler, INavigationService nav, PlantProjection plantProjection)
            : base(messenger, ctx, handler, nav)
        {
            this.PlantProjection = plantProjection;
            this.PlantProjection.EventHandled += this.EventHandled;

            this.Plants = new ObservableCollection<PlantCreated>();
        }

        public void LoadPlants()
        {
            if (this.Loaded)
                return;
            var r = this.PlantProjection.LoadWithUserId(this.Context.CurrentUser.Id).ToArray();
            foreach (var e in r)
            {
                this.Plants.Add(e);
            }
            this.Loaded = true;
        }

        private void EventHandled(object sender, EventHandledArgs e)
        {
            var @event = e.@event as PlantCreated;
            if (@event != null && @event.UserId == this.Context.CurrentUser.Id)
                this.Plants.Add(@event);
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

        private RelayCommand _AddPlantCommand;
        public RelayCommand AddPlantCommand
        {
            get
            {

                if (_AddPlantCommand == null)
                    _AddPlantCommand = new RelayCommand(() =>
                    {

                        AddPlant(NewPlantName, NewPlantId);
                        NewPlantName = "";
                        Nav.GoBack();
                    });
                return _AddPlantCommand;

            }
        }


        public void AddPlant(string name, Guid plantId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("name");


            Handler.Handle<Plant, CreatePlant>(new CreatePlant(plantId, name, this.Context.CurrentUser.Id));
            Handler.Handle<Garden, AddPlant>(new AddPlant(this.Context.CurrentUser.GardenId, plantId, name));

        }
        #endregion

        private PlantCreated _SelectedPlant;
        public PlantCreated SelectedPlant
        {
            get { return _SelectedPlant; }
            private set
            {
                Set(() => SelectedPlant, ref _SelectedPlant, value);
            }
        }
        //public event EventHandler<SelectedPlantArgs> PlantSelected;
        private RelayCommand<PlantCreated> _ShowDetailsCommand;
        public RelayCommand<PlantCreated> ShowDetailsCommand
        {
            get
            {

                if (_ShowDetailsCommand == null)
                    _ShowDetailsCommand = new RelayCommand<PlantCreated>(p =>
                    {
                        this.SelectedPlant = p;
                        MessengerInstance.Send(new ShowPlantView(p));
                        Nav.NavigateTo(View.PLANT);
                    });
                return _ShowDetailsCommand;

            }
        }
    }
}