
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

        protected readonly IPlantViewModel Current;
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
                if (current.WateringSchedule.Interval.HasValue && current.WateringSchedule.Interval.Value.TotalSeconds > 0)
                    this.WateringSchedule.IsEnabled = true;
                else
                    this.WateringSchedule.IsEnabled = false;
                this.FertilizingSchedule = current.FertilizingSchedule;
                if (current.FertilizingSchedule.Interval.HasValue && current.FertilizingSchedule.Interval.Value.TotalSeconds > 0)
                    this.FertilizingSchedule.IsEnabled = true;
                else
                    this.FertilizingSchedule.IsEnabled = false;
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


            this.WateringSchedule
                .WhenAny(x => x.IsEnabled, x => Tuple.Create(this.WateringSchedule, x.GetValue()))
                .Skip(1)
                .Merge(
                    this.FertilizingSchedule
                        .WhenAny(x => x.IsEnabled, x => Tuple.Create(this.FertilizingSchedule, x.GetValue()))
                        .Skip(1)
                )
                .Where(x => x.Item2 == true)
                .Subscribe(x => App.Router.Navigate.Execute(x.Item1));


            //this.CanExecute = this.WhenAnyValue(x => x.Name, x => x.Species, x => x.ProfilepicturePath, IsValid);

            this.CanExecute = this.WhenAnyValue(
                       x => x.Name,
                       x => x.Species,
                       x => x.FertilizingSchedule.Interval,
                       x => x.WateringSchedule.Interval,
                       x => x.Photo,
                       x => x.Tags,
                       this.IsValid
                    );


            this.AddCommand.RegisterAsyncTask((_) => this.AddTask()).Publish().Connect();


        }

        protected bool AnyChange(string name, string species, TimeSpan? fert, TimeSpan? water, Photo pic, IList<string> tags, bool isShared)
        {
            int changes = 0;
            if (Current.Species != species)
            {
                changes++;
            }
            if (Current.Name != name)
            {
                changes++;
            }
            if (this.FertilizingSchedule.HasChanged)
            {
                changes++;
            }
            if (this.WateringSchedule.HasChanged)
            {
                changes++;
            }
            if (Current.Photo != pic)
            {
                changes++;
            }
            if (Current.IsShared != isShared)
            {
                changes++;
            }
            if (!TagSet.SetEquals(tags))
            {
                changes++;
            }
            return changes > 0;
        }

        protected bool IsValid(string name, string species, TimeSpan? fert, TimeSpan? water, Photo pic, IList<string> tags)
        {
            int valid = 0;
            if (!string.IsNullOrWhiteSpace(name))
                valid++;
            if (!string.IsNullOrWhiteSpace(species))
                valid++;
            //if (pic != null)
            //   valid++;
            //if (fert != default(Guid))
            //    valid++;
            //if (water != default(Guid))
            //   valid++;

            if (valid < 2)
                return false;

            if (Current != null)
                return AnyChange(name, species, fert, water, pic, tags, this.IsShared);

            return true;
        }

        //protected bool AnyChange(string name, string species, IScheduleViewModel fert, IScheduleViewModel water, Photo pic, IList<string> tags)
        //{
        //    int changes = 0;
        //    if (Current.Species != species)
        //    {
        //        changes++;
        //    }
        //    if (Current.Name != name)
        //    {
        //        changes++;
        //    }
        //    if (fert.HasChanged)
        //    {
        //        changes++;
        //    }
        //    if (water.HasChanged)
        //    {
        //        changes++;
        //    }
        //    if (Current.Photo != pic)
        //    {
        //        changes++;
        //    }
        //    if (!TagSet.SetEquals(tags))
        //    {
        //        changes++;
        //    }
        //    return changes > 0;
        //}

        //protected bool IsValid(string name, string species, IScheduleViewModel fert, IScheduleViewModel water, Photo pic, IList<string> tags)
        //{
        //    int valid = 0;
        //    if (!string.IsNullOrWhiteSpace(name))
        //        valid++;
        //    if (!string.IsNullOrWhiteSpace(species))
        //        valid++;
        //    //if (pic != null)
        //    //   valid++;
        //    //if (fert != default(Guid))
        //    //    valid++;
        //    //if (water != default(Guid))
        //    //   valid++;

        //    if (valid < 2)
        //        return false;

        //    if (Current != null)
        //        return AnyChange(name, species, fert, water, pic, tags);

        //    return true;
        //}

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

            if (this.FertilizingSchedule.HasChanged)
            {
                var r = await this.FertilizingSchedule.Create();
                if (r != null || this.FertilizingSchedule.Id == null)
                    await App.HandleCommand(new SetFertilizingSchedule(plantId, this.FertilizingSchedule.Id));
            }
            if (this.WateringSchedule.HasChanged)
            {
                var r = await this.WateringSchedule.Create();
                if (r != null || this.WateringSchedule.Id == null)
                    await App.HandleCommand(new SetWateringSchedule(plantId, this.WateringSchedule.Id));
            }
            if (this.Photo != null && current.Photo != this.Photo)
            {
                var plantActionId = Guid.NewGuid();

                if (current.Actions.Count == 0)
                {
                    await App.HandleCommand(new CreatePlantAction(plantActionId, App.User.Id, plantId, PlantActionType.PHOTOGRAPHED, null) { Photo = this.Photo });

                }
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