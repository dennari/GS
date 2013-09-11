
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

    public interface IPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public class PlantViewModel : RoutableViewModel, IPlantViewModel
    {
        public ActionProjection ActionProjection { get; private set; }

        public ObservableCollection<ActionBase> Actions { get; private set; }


        public ReactiveList<ButtonViewModel> AppBarButtons { get; protected set; }


        public string AppBarMode { get { return GSApp.APPBAR_MODE_DEFAULT; } }
        public bool AppBarIsVisible { get { return true; } }


        protected readonly Guid Id;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PlantViewModel(Guid id, IUserService ctx, IMessageBus handler, IScreen host)
            : base(ctx, handler, host)
        {
            //this.ActionProjection = actionProjection;
            //this.ActionProjection.EventHandled += this.ActionHandled;
            //this.Actions = new ObservableCollection<ActionBase>();
            this.Id = id;
            this.AppBarButtons = new ReactiveList<ButtonViewModel>();

        }



        private PlantCreated _Plant;
        public PlantCreated Plant
        {
            get { return _Plant; }
            private set
            {
                if (_Plant == value)
                    return;

                this.RaiseAndSetIfChanged(ref _Plant, value);
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


        private ReactiveCommand _AddCommentCommand;
        public ReactiveCommand AddCommentCommand
        {
            get
            {

                if (_AddCommentCommand == null)
                {
                    _AddCommentCommand = new ReactiveCommand();
                    _AddCommentCommand.Subscribe((note) =>
                    {
                        this.Add(new Comment(Context.CurrentUser.Id, this.Plant.EntityId, (string)note));
                    });
                }
                return _AddCommentCommand;

            }
        }

        private ReactiveCommand _AddPhotoCommand;
        public ReactiveCommand AddPhotoCommand
        {
            get
            {

                if (_AddPhotoCommand == null)
                {
                    _AddPhotoCommand = new ReactiveCommand();
                    _AddPhotoCommand.Subscribe((photo) =>
                    {
                        this.Add(new Photograph(Context.CurrentUser.Id, this.Plant.EntityId, "", photo as Uri));
                    });
                }
                return _AddPhotoCommand;

            }
        }


        private ReactiveCommand _AddFertilizerCommand;
        public ReactiveCommand AddFertilizerCommand
        {
            get
            {

                if (_AddFertilizerCommand == null)
                {
                    _AddFertilizerCommand = new ReactiveCommand();
                    _AddFertilizerCommand.Subscribe(_ =>
                    {
                        this.Add(new Fertilize(Context.CurrentUser.Id, this.Plant.EntityId, ""));
                    });
                }
                return _AddFertilizerCommand;

            }
        }

        private ReactiveCommand _AddWaterCommand;
        public ReactiveCommand AddWaterCommand
        {
            get
            {

                if (_AddWaterCommand == null)
                {
                    _AddWaterCommand = new ReactiveCommand();
                    _AddWaterCommand.Subscribe(_ =>
                    {
                        this.Add(new Water(Context.CurrentUser.Id, this.Plant.EntityId, ""));
                    });
                }
                return _AddWaterCommand;

            }
        }


        public void Add(Comment command)
        {
            Bus.Handle(command);
        }

        public void Add(Fertilize command)
        {
            Bus.Handle(command);
        }

        public void Add(Water command)
        {
            Bus.Handle(command);

        }

        public void Add(Photograph command)
        {
            Bus.Handle(command);

        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }


    }
}