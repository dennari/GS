
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using System;
using System.Collections.Generic;


namespace Growthstories.UI.ViewModel
{

    public interface IAddPlantViewModel : IGSRoutableViewModel, IHasAppBarButtons, IControlsAppBar
    {

    }

    public enum ScheduleType
    {
        WATERING,
        FERTILIZING
    }

    public class AddPlantViewModel : PlantActionViewModel, IAddPlantViewModel
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

        protected new PlantState State;

        public AddPlantViewModel(PlantState state, IGSApp app)
            : base(null, app)
        {
            this.State = state;
            this.Title = "new";

            ChooseProfilePictureCommand = new ReactiveCommand();
            ChooseWateringSchedule = new ReactiveCommand();
            ChooseWateringSchedule.Subscribe(_ => App.Router.Navigate.Execute(this.WateringSchedule));
            ChooseFertilizingSchedule = new ReactiveCommand();
            ChooseFertilizingSchedule.Subscribe(_ => App.Router.Navigate.Execute(this.FertilizingSchedule));

            Tags = new ReactiveList<string>();
            AddTag = new ReactiveCommand();
            AddTag.OfType<string>().Subscribe(x =>
            {
                if (!this.Tags.Contains(x))
                    this.Tags.Add(x);
            });

            RemoveTag = new ReactiveCommand();
            RemoveTag.OfType<string>().Subscribe(x =>
            {
                this.Tags.Remove(x);
            });

            this.WhenAny(x => x.Name, x => x.Species, (name, species) => this.FormatPlantTitle(name.GetValue(), species.GetValue()))
                .ToProperty(this, x => x.PlantTitle, out this._PlantTitle, null);



            //this.CanExecute = this.WhenAnyValue(x => x.Name, x => x.Species, x => x.ProfilepicturePath, IsValid);

            this.CanExecute = this.WhenAnyValue(
                       x => x.Name,
                       x => x.Species,
                       x => x.FertilizingSchedule.Id,
                       x => x.WateringSchedule.Id,
                       x => x.ProfilepictureData,
                       x => x.Tags,
                       this.IsValid
                    );

            if (State != null)
            {
                this.Name = State.Name;
                this.Species = State.Species;
                this.Tags = new ReactiveList<string>(State.Tags);
                this.Title = "edit";
                this.ProfilepictureData = State.Profilepicture;
                this.ProfilePictureButtonText = "";

            }

        }

        protected bool AnyChange(string name, string species, Guid fert, Guid water, Photo pic, IList<string> tags)
        {
            int changes = 0;
            if (State.Species != species)
            {
                changes++;
            }
            if (State.Name != name)
            {
                changes++;
            }
            if (State.FertilizingScheduleId != fert)
            {
                changes++;
            }
            if (State.WateringScheduleId != water)
            {
                changes++;
            }
            if (State.Profilepicture != pic)
            {
                changes++;
            }
            if (!State.Tags.SetEquals(tags))
            {
                changes++;
            }
            return changes > 0;
        }

        protected bool IsValid(string name, string species, Guid fert, Guid water, Photo pic, IList<string> tags)
        {
            int valid = 0;
            if (!string.IsNullOrWhiteSpace(name))
                valid++;
            if (!string.IsNullOrWhiteSpace(species))
                valid++;
            if (pic != default(Photo))
                valid++;
            if (fert != default(Guid))
                valid++;
            if (water != default(Guid))
                valid++;

            if (valid < 5)
                return false;

            if (State != null)
                return AnyChange(name, species, fert, water, pic, tags);

            return true;
        }

        public ReactiveCommand ChooseProfilePictureCommand { get; protected set; }

        public ReactiveCommand ChooseWateringSchedule { get; protected set; }

        public ReactiveCommand AddTag { get; protected set; }
        public ReactiveCommand RemoveTag { get; protected set; }


        public ReactiveCommand ChooseFertilizingSchedule { get; protected set; }

        public ReactiveList<string> Tags { get; protected set; }

        protected ScheduleViewModel _WateringSchedule;
        public ScheduleViewModel WateringSchedule
        {
            get
            {
                if (_WateringSchedule == null)
                {
                    _WateringSchedule = App.ScheduleViewModelFactory(this.State, ScheduleType.WATERING);
                }
                return _WateringSchedule;
            }
        }


        protected ScheduleViewModel _FertilizingSchedule;
        public ScheduleViewModel FertilizingSchedule
        {
            get
            {
                if (_FertilizingSchedule == null)
                {
                    _FertilizingSchedule = App.ScheduleViewModelFactory(this.State, ScheduleType.FERTILIZING);
                }
                return _FertilizingSchedule;
            }
        }


        protected string _ProfilePictureButtonText = "select";
        public string ProfilePictureButtonText
        {
            get
            {
                return _ProfilePictureButtonText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ProfilePictureButtonText, value);
            }
        }

        protected string FormatPlantTitle(string name, string species)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            return string.IsNullOrWhiteSpace(species) ? name.ToUpper() : string.Format("{0} ({1})", name.ToUpper(), species.ToUpper()); ;
        }

        private ObservableAsPropertyHelper<string> _PlantTitle;
        public new string PlantTitle
        {
            get
            {
                return this._PlantTitle.Value;
            }
        }

        protected string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Name, value);
            }
        }

        protected string _Species;
        public string Species
        {
            get
            {
                return _Species;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Species, value);
            }
        }

        protected Guid _Id;
        public Guid Id
        {
            get
            {
                return _Id == default(Guid) ? Guid.NewGuid() : _Id;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Id, value);
            }
        }

        protected Photo _ProfilepictureData;
        public Photo ProfilepictureData
        {
            get
            {
                return _ProfilepictureData;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ProfilepictureData, value);
            }
        }


        public override void AddCommandSubscription(object p)
        {
            //IEntityCommand cmd = null;
            if (this.State != null)
            {
                if (State.Species != this.Species)
                {
                    this.SendCommand(new SetSpecies(State.Id, this.Species));
                }
                if (State.Name != this.Name)
                {
                    this.SendCommand(new SetName(State.Id, this.Name));
                }
                if (State.FertilizingScheduleId != this.FertilizingSchedule.Id)
                {
                    this.SendCommand(new SetFertilizingSchedule(State.Id, this.FertilizingSchedule.Id));
                }
                if (State.WateringScheduleId != this.WateringSchedule.Id)
                {
                    this.SendCommand(new SetWateringSchedule(State.Id, this.WateringSchedule.Id));
                }
                if (State.Profilepicture != this.ProfilepictureData)
                {
                    this.SendCommand(new SetProfilepicture(State.Id, this.ProfilepictureData));
                }
                if (!State.Tags.SetEquals(this.Tags))
                {
                    this.SendCommand(new SetTags(State.Id, new HashSet<string>(this.Tags)));
                }
                this.App.Router.NavigateBack.Execute(null);
            }
            else
            {
                var plantId = Guid.NewGuid();
                this.SendCommand(new CreatePlant(plantId, this.Name, App.Context.CurrentUser.Id)
                {
                    Species = this.Species,
                    Profilepicture = this.ProfilepictureData,
                    FertilizingScheduleId = this.FertilizingSchedule.Id,
                    WateringScheduleId = this.WateringSchedule.Id,
                    Tags = new HashSet<string>(this.Tags)
                }, false);

                this.SendCommand(new AddPlant(App.Context.CurrentUser.GardenId, plantId, this.Name), true);


            }
        }


    }
}