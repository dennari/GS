using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Growthstories.UI.ViewModel
{


    public class PlantViewModel : GSViewModelBase, IHandles<ShowPlantView>
    {
        public ActionProjection ActionProjection { get; private set; }

        public ObservableCollection<ActionBase> Actions { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantViewModel(IMessenger messenger, IUserService ctx, IMessageBus handler, INavigationService nav, ActionProjection actionProjection)
            : base(messenger, ctx, handler, nav)
        {
            this.ActionProjection = actionProjection;
            this.ActionProjection.EventHandled += this.ActionHandled;
            this.Actions = new ObservableCollection<ActionBase>();
        }



        private PlantCreated _Plant;
        public PlantCreated Plant
        {
            get { return _Plant; }
            private set
            {
                if (_Plant == value)
                    return;

                Set(() => Plant, ref _Plant, value);
                this.LoadActions(value.EntityId);
            }
        }

        private void ActionHandled(object sender, EventHandledArgs e)
        {
            var action = e.@event as ActionBase;
            if (action != null && this.Plant != null && (action.PlantId == this.Plant.EntityId || action.EntityId == this.Plant.UserId))
                this.Actions.Add(action);
        }

        public void LoadActions(Guid plantId)
        {
            this.Actions.Clear();
            foreach (var a in this.ActionProjection.LoadWithPlantId(plantId))
                this.Actions.Add(a);
        }


        private RelayCommand<string> _AddCommentCommand;
        public RelayCommand<string> AddCommentCommand
        {
            get
            {

                if (_AddCommentCommand == null)
                    _AddCommentCommand = new RelayCommand<string>((note) =>
                    {
                        this.Add(new Comment(Context.CurrentUser.Id, this.Plant.EntityId, note));
                    });
                return _AddCommentCommand;

            }
        }

        private RelayCommand<object> _AddPhotoCommand;
        public RelayCommand<object> AddPhotoCommand
        {
            get
            {

                if (_AddPhotoCommand == null)
                    _AddPhotoCommand = new RelayCommand<object>((photo) =>
                    {
                        this.Add(new Photograph(Context.CurrentUser.Id, this.Plant.EntityId, "", photo as Uri));
                    });
                return _AddPhotoCommand;

            }
        }


        private RelayCommand _AddFertilizerCommand;
        public RelayCommand AddFertilizerCommand
        {
            get
            {

                if (_AddFertilizerCommand == null)
                    _AddFertilizerCommand = new RelayCommand(() =>
                    {
                        this.Add(new Fertilize(Context.CurrentUser.Id, this.Plant.EntityId, ""));
                    });
                return _AddFertilizerCommand;

            }
        }

        private RelayCommand _AddWaterCommand;
        public RelayCommand AddWaterCommand
        {
            get
            {

                if (_AddWaterCommand == null)
                    _AddWaterCommand = new RelayCommand(() =>
                    {
                        this.Add(new Water(Context.CurrentUser.Id, this.Plant.EntityId, ""));
                    });
                return _AddWaterCommand;

            }
        }


        public void Add(Comment command)
        {
            Handler.Handle(command);
        }

        public void Add(Fertilize command)
        {
            Handler.Handle(command);
        }

        public void Add(Water command)
        {
            Handler.Handle(command);

        }

        public void Add(Photograph command)
        {
            Handler.Handle(command);

        }


        public void Handle(ShowPlantView @event)
        {
            this.Plant = @event.SelectedPlant;
        }
    }
}