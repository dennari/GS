
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Growthstories.UI.ViewModel
{



    public class AddEditPlantViewModel : CommandViewModel, IAddEditPlantViewModel
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

        public IPlantViewModel Current { get; protected set; }
        protected readonly HashSet<string> TagSet;

        public AddEditPlantViewModel(IGSAppViewModel app, IPlantViewModel current = null)
            : base(app)
        {


            if (current != null)
            {
                //var state = current.State;
                this.Name = current.Name;
                this.Species = current.Species;
                this.TagSet = new HashSet<string>(current.Tags ?? new ReactiveList<string>());
                this.Tags = current.Tags ?? new ReactiveList<string>();
                this.Title = "edit plant";
                this.Photo = current.Photo;
                this.ProfilePictureButtonText = "";
                this.Current = current;
                this.IsShared = current.IsShared;
                this.WateringSchedule = current.WateringSchedule;
                this.FertilizingSchedule = current.FertilizingSchedule;
                this.IsWateringScheduleEnabled = current.IsWateringScheduleEnabled;
                this.IsFertilizingScheduleEnabled = current.IsFertilizingScheduleEnabled;

            }
            else
            {
                this.Title = "new plant";
                this.TagSet = new HashSet<string>();
                this.Tags = new ReactiveList<string>();
            }
            if (this.WateringSchedule == null)
                this.WateringSchedule = new ScheduleViewModel(null, ScheduleType.WATERING, app);
            if (this.FertilizingSchedule == null)
                this.FertilizingSchedule = new ScheduleViewModel(null, ScheduleType.FERTILIZING, app);


            // a little hack to forward the enabled flag to the schedule viewmodels
            ScheduleViewModel schedule = this.WateringSchedule as ScheduleViewModel;
            this.WhenAnyValue(x => x.IsWateringScheduleEnabled).ToProperty(schedule, x => x.IsEnabled, out schedule._IsEnabled, this.IsWateringScheduleEnabled);
            schedule = this.FertilizingSchedule as ScheduleViewModel;
            this.WhenAnyValue(x => x.IsFertilizingScheduleEnabled).ToProperty(schedule, x => x.IsEnabled, out schedule._IsEnabled, this.IsFertilizingScheduleEnabled);



            var garden = app.Resolver.GetService<IGardenViewModel>();
            this.WateringSchedule.OtherSchedules = new ReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>>(
                garden.Plants.Where(x => x.WateringSchedule.Interval.HasValue && (this.Current == null || this.Current.Id != x.Id)).Select(x =>
                {
                    return Tuple.Create(x, x.WateringSchedule);
                })
            );
            this.FertilizingSchedule.OtherSchedules = new ReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>>(
             garden.Plants.Where(x => x.FertilizingSchedule.Interval.HasValue).Select(x => Tuple.Create(x, x.FertilizingSchedule))
           );


            ChooseProfilePictureCommand = new ReactiveCommand();
            //ChooseWateringSchedule = new ReactiveCommand();
            //ChooseWateringSchedule.Subscribe(_ => App.Router.Navigate.Execute(this.WateringSchedule));
            //ChooseFertilizingSchedule = new ReactiveCommand();
            //ChooseFertilizingSchedule.Subscribe(_ => App.Router.Navigate.Execute(this.FertilizingSchedule));

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

            //this.WhenAny(x => x.Name, x => x.Species, (name, species) => this.FormatPlantTitle(name.GetValue(), species.GetValue()))
            //    .ToProperty(this, x => x.PlantTitle, out this._PlantTitle, null);

            this.WhenAnyValue(x => x.IsWateringScheduleEnabled)
                .Skip(1)
                .Where(x => x)
                .Subscribe(_ => this.Navigate(this.WateringSchedule));
            this.WhenAnyValue(x => x.IsFertilizingScheduleEnabled)
                .Skip(1)
                .Where(x => x)
                .Subscribe(_ => this.Navigate(this.FertilizingSchedule));


            this.CanExecute = Observable.Merge(
                    this.Changed,
                    this.WateringSchedule.Changed,
                    this.FertilizingSchedule.Changed)
                .Select(_ => this.IsValid());

            this.AddCommand.RegisterAsyncTask((_) => this.AddTask()).Publish().Connect();


        }

        protected bool AnyChange()
        {
            int changes = 0;
            if (Current.Species != Species)
            {
                changes++;
            }
            if (Current.Name != Name)
            {
                changes++;
            }
            if (Current.IsFertilizingScheduleEnabled != this.IsFertilizingScheduleEnabled)
                changes++;
            if (Current.IsWateringScheduleEnabled != this.IsWateringScheduleEnabled)
                changes++;
            if (this.FertilizingSchedule.HasChanged)
            {
                changes++;
            }
            if (this.WateringSchedule.HasChanged)
            {
                changes++;
            }
            if (Current.Photo != Photo)
            {
                changes++;
            }
            if (Current.IsShared != IsShared)
            {
                changes++;
            }
            if (!TagSet.SetEquals(Tags))
            {
                changes++;
            }
            return changes > 0;
        }

        protected bool IsValid()
        {
            int valid = 0;
            if (!string.IsNullOrWhiteSpace(Name))
                valid++;
            if (!string.IsNullOrWhiteSpace(Species))
                valid++;


            if (valid < 2)
                return false;

            if (Current != null)
                return AnyChange();

            return true;
        }


        public IReactiveCommand ChooseProfilePictureCommand { get; protected set; }

        public IReactiveCommand ChooseWateringSchedule { get; protected set; }

        public IReactiveCommand AddTag { get; protected set; }
        public IReactiveCommand RemoveTag { get; protected set; }


        public IReactiveCommand ChooseFertilizingSchedule { get; protected set; }

        public IReactiveList<string> Tags { get; protected set; }

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
        public string PlantTitle
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

        public override void AddCommandSubscription(object p)
        { }

        public async Task<Guid> AddTask()
        {
            //IEntityCommand cmd = null;




            var plantId = Current == null ? Guid.NewGuid() : Current.Id;

            // why is the plantviewmodel instantiated with a null AppViewModel?
            //   -- JOJ 5.1.2014

            IPlantViewModel current = Current == null ? new PlantViewModel(null, null) : Current;
            IPlantViewModel R = Current == null ? null : Current;

            if (this.Current == null)
            {

                await App.HandleCommand(new CreatePlant(plantId, this.Name, App.User.GardenId, App.User.Id));
                await App.HandleCommand(new AddPlant(App.User.GardenId, plantId, App.User.Id, this.Name));
            }
            else if (current.Name != this.Name)
            {
                await App.HandleCommand(new SetName(plantId, this.Name));
            }

            if (current.Species != this.Species)
            {
                await App.HandleCommand(new SetSpecies(plantId, this.Species));
            }
            if (current.IsShared != this.IsShared)
            {
                if (this.IsShared == true)
                {
                    await App.HandleCommand(new MarkPlantPublic(plantId));

                }
                else
                {
                    await App.HandleCommand(new MarkPlantPrivate(plantId));

                }
            }
            if (current.IsWateringScheduleEnabled != this.IsWateringScheduleEnabled)
                await App.HandleCommand(new ToggleSchedule(plantId, this.IsWateringScheduleEnabled, ScheduleType.WATERING));

            if (current.IsFertilizingScheduleEnabled != this.IsFertilizingScheduleEnabled)
                await App.HandleCommand(new ToggleSchedule(plantId, this.IsFertilizingScheduleEnabled, ScheduleType.FERTILIZING));

            IScheduleViewModel schedule = this.FertilizingSchedule;
            if ((!schedule.Id.HasValue || schedule.HasChanged) && this.IsFertilizingScheduleEnabled)
            {
                var r = await schedule.Create();
                await App.HandleCommand(new SetFertilizingSchedule(plantId, r.Id));
            }
            schedule = this.WateringSchedule;
            if ((!schedule.Id.HasValue || schedule.HasChanged) && this.IsWateringScheduleEnabled)
            {
                var r = await schedule.Create();
                await App.HandleCommand(new SetWateringSchedule(plantId, r.Id));
            }

            if (this.Photo != null && current.Photo != this.Photo && current.Actions.Count == 0)
            {
                var plantActionId = Guid.NewGuid();
                await App.HandleCommand(new CreatePlantAction(plantActionId, App.User.Id, plantId, PlantActionType.PHOTOGRAPHED, null) { Photo = this.Photo });
                await App.HandleCommand(new SchedulePhotoUpload(this.Photo, plantActionId));
                await App.HandleCommand(new SetProfilepicture(plantId, this.Photo, plantActionId));

            }
            //this.Ta
            if (!this.TagSet.SetEquals(this.Tags))
            {
                await App.HandleCommand(new SetTags(plantId, new HashSet<string>(this.Tags)));
            }
            if (this.App.Router.NavigationStack.Count > 1)
                this.App.Router.NavigateBack.Execute(null);
            return plantId;
        }



        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }
}