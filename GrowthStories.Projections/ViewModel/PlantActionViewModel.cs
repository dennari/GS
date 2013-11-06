using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{



    public abstract class CommandViewModel : RoutableViewModel, ICommandViewModel, IControlsAppBar
    {
        protected string _TopTitle;
        public string TopTitle { get { return _TopTitle; } protected set { this.RaiseAndSetIfChanged(ref _TopTitle, value); } }

        protected string _Title;
        public string Title { get { return _Title; } protected set { this.RaiseAndSetIfChanged(ref _Title, value); } }

        public CommandViewModel(IGSAppViewModel app)
            : base(app)
        { }

        protected ReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<IButtonViewModel>()
                    {
                        new ButtonViewModel(null)
                        {
                            Text = "save",
                            IconUri = App.IconUri[IconType.CHECK],
                            Command = AddCommand
                        }
                    };
                return _AppBarButtons;
            }
        }
        private ReactiveCommand _AddCommand;
        public IReactiveCommand AddCommand
        {
            get
            {

                if (_AddCommand == null)
                {
                    _AddCommand = new ReactiveCommand(this.CanExecute == null ? Observable.Return(true) : this.CanExecute, false);
                    _AddCommand.Subscribe(this.AddCommandSubscription);

                }
                return _AddCommand;

            }
        }

        public virtual void AddCommandSubscription(object p)
        {

        }

        public IObservable<bool> CanExecute { get; protected set; }


        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return true; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }
    }




    public abstract class PlantActionViewModel : CommandViewModel, IPlantActionViewModel
    {

        public PlantActionState State { get; protected set; }

        public string WeekDay { get; protected set; }
        public string Date { get; protected set; }
        public string Time { get; protected set; }
        public PlantActionType ActionType { get; protected set; }



        public Uri IconUri { get { return this.App.BigIconUri[this._IconType]; } }

        protected IconType _IconType;



        public Guid PlantActionId { get; protected set; }
        public DateTimeOffset Created { get; protected set; }

        public PlantActionViewModel(PlantActionState state, IGSAppViewModel app)
            : base(app)
        {

            this.State = state;
            if (state != null)
            {
                this.Note = state.Note;
                this.WeekDay = state.Created.ToString("dddd");
                this.Date = state.Created.ToString("d");
                this.Time = state.Created.ToString("t");
                this.PlantActionId = state.Id;
                this.Created = state.Created;
                this.ActionType = state.Type;
                this.ListenTo<PlantActionPropertySet>(state.Id).Subscribe(x =>
                {
                    SetProperty(x);
                });

            }

        }

        public PlantActionViewModel(DateTimeOffset Created, IGSAppViewModel app)
            : base(app)
        {


            this.Note = "Just a note";
            this.WeekDay = Created.ToString("dddd");
            this.Date = Created.ToString("d");
            this.Time = Created.ToString("t");
            this.PlantActionId = Guid.NewGuid();
            this.Created = Created;
        }

        public virtual void SetProperty(PlantActionPropertySet prop)
        {
            if (this.State == null || prop.AggregateId != this.PlantActionId || prop.Type != this.State.Type)
                throw new InvalidOperationException();
            if (prop.Note != null)
                this.Note = prop.Note;
        }

        protected string _Note;
        public string Note
        {
            get
            {
                return _Note;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Note, value);
            }
        }





    }



    public sealed class MeasurementTypeViewModel : GSViewModelBase
    {

        private IconType _Icon;

        public MeasurementTypeViewModel(MeasurementType type, string title, IconType icon, IGSAppViewModel app)
            : base(app)
        {
            this.Type = type;
            this.Title = title;
            this._Icon = icon;


        }

        public MeasurementType Type { get; set; }

        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }

        public Uri Icon { get { return App != null ? App.IconUri[this._Icon] : new Uri("/Assets/Icons/icon_length_appbar.png", UriKind.RelativeOrAbsolute); } }

        public static IList<MeasurementTypeViewModel> GetAll(IGSAppViewModel app)
        {
            return new List<MeasurementTypeViewModel>()
            {
                new MeasurementTypeViewModel(MeasurementType.ILLUMINANCE,"Illuminance",IconType.MEASURE,app),
                new MeasurementTypeViewModel(MeasurementType.LENGTH,"Length",IconType.MEASURE,app),
                new MeasurementTypeViewModel(MeasurementType.PH,"PH",IconType.MEASURE,app),
                new MeasurementTypeViewModel(MeasurementType.SOIL_HUMIDITY,"Soil Humidity",IconType.MEASURE,app),
                new MeasurementTypeViewModel(MeasurementType.WEIGHT,"Weight",IconType.MEASURE,app)
            };
        }

    }





    public class PlantCommentViewModel : PlantActionViewModel, IPlantCommentViewModel
    {


        public new string Title { get { return "COMMENTED"; } }

        public PlantCommentViewModel(PlantActionState state, IGSAppViewModel app)
            : base(state, app)
        {

            if (state != null && state.Type != PlantActionType.COMMENTED)
                throw new InvalidOperationException();
            this._IconType = IconType.NOTE;
            this.CanExecute = this.WhenAnyValue(x => x.Note, x => !string.IsNullOrWhiteSpace(x));
        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Comment(this.State.EntityId, this.State.PlantId, this.Note), true);
        }

    }

    public class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {


        public new string Title { get { return "measure"; } }

        public IList<MeasurementTypeViewModel> MeasurementTypes { get; protected set; }


        protected ObservableAsPropertyHelper<MeasurementTypeViewModel> _Series;
        public MeasurementTypeViewModel Series
        {
            get { return _Series.Value; }
        }

        public ReactiveCommand SeriesSelected { get; protected set; }

        protected string _SValue;
        public string SValue
        {
            get { return _SValue; }
            set { this.RaiseAndSetIfChanged(ref _SValue, value); }
        }

        protected double? _Value;
        public double? Value
        {
            get { return _Value; }
            set { this.RaiseAndSetIfChanged(ref _Value, value); }
        }

        //public string Series { get { return ((Measured)this.State).Series.ToString("G"); } }
        //public string Value { get { return ((Measured)this.State).Value.ToString("F1"); } }

        public override void AddCommandSubscription(object p)
        {


        }


        public PlantMeasureViewModel(PlantActionState state, IGSAppViewModel app)
            : base(state, app)
        {
            if (state != null && state.Type != PlantActionType.MEASURED)
                throw new InvalidOperationException();

            this._IconType = IconType.MEASURE;
            this.MeasurementTypes = MeasurementTypeViewModel.GetAll(app);
            this.SeriesSelected = new ReactiveCommand();
            this.SeriesSelected
                .OfType<MeasurementTypeViewModel>()
                .ToProperty(this, x => x.Series, out _Series, state != null ? MeasurementTypes.FirstOrDefault(x => x.Type == state.MeasurementType) : MeasurementTypes[0]);





            double dValue = 0;
            this.WhenAnyValue(x => x.SValue, x => x)
                .Where(x => double.TryParse(x, out dValue))
                .Subscribe(x => this.Value = dValue);

            this.CanExecute = this.WhenAnyValue(x => x.Value, x => x.HasValue);

            if (state != null)
            {
                this.SValue = state.Value.Value.ToString("F1");
                this.Value = state.Value;
            }
        }

        public override void SetProperty(PlantActionPropertySet prop)
        {
            base.SetProperty(prop);
            this.Value = prop.Value;
            this.SValue = prop.Value.Value.ToString("F1");

        }

    }

    public class PlantWaterViewModel : PlantActionViewModel, IPlantWaterViewModel
    {

        public new string Title { get { return "WATERED"; } }


        public PlantWaterViewModel(PlantActionState state, IGSAppViewModel app)
            : base(state, app)
        {
            if (state != null && state.Type != PlantActionType.WATERED)
                throw new InvalidOperationException();

            this._IconType = IconType.WATER;
        }

        public PlantWaterViewModel(DateTimeOffset created, IGSAppViewModel app)
            : base(created, app)
        {

            this._IconType = IconType.WATER;
        }

        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Water(this.State.EntityId, this.State.PlantId, this.Note), true);
        }
    }

    public class PlantFertilizeViewModel : PlantActionViewModel, IPlantFertilizeViewModel
    {

        public new string Title { get { return "NOURISHED"; } }

        public PlantFertilizeViewModel(PlantActionState state, IGSAppViewModel app)
            : base(state, app)
        {
            if (state != null && state.Type != PlantActionType.FERTILIZED)
                throw new InvalidCastException();
            this._IconType = IconType.FERTILIZE;
        }

        public PlantFertilizeViewModel(DateTimeOffset created, IGSAppViewModel app)
            : base(created, app)
        {

            this._IconType = IconType.FERTILIZE;
        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Fertilize(this.State.EntityId, this.State.PlantId, this.Note), true);
        }

    }


    public class PlantPhotographViewModel : PlantActionViewModel, IPlantPhotographViewModel
    {
        public new string Title { get { return "PHOTOGRAPHED"; } }

        protected Photo _PhotoData;
        public Photo PhotoData
        {
            get
            {
                return _PhotoData;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PhotoData, value);
            }
        }



        public PlantPhotographViewModel(PlantActionState state, IGSAppViewModel app)
            : base(state, app)
        {

            if (state != null && state.Type != PlantActionType.PHOTOGRAPHED)
                throw new InvalidCastException();
            this._IconType = IconType.PHOTO;


            if (state != null)
            {
                this.PhotoData = state.Photo.Value;
            }
        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Photograph(this.State.EntityId, this.State.PlantId, this.Note, this.Path), true);
        }


        public override void SetProperty(PlantActionPropertySet prop)
        {
            base.SetProperty(prop);
            this.PhotoData = prop.Photo.Value;
        }


    }

}
