using Growthstories.Domain.Messaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{


    public interface ICommandViewModel : IGSRoutableViewModel, IHasAppBarButtons
    {
        ReactiveCommand AddCommand { get; }
        IObservable<bool> CanExecute { get; }
        string TopTitle { get; }
        string Title { get; }

    }

    public abstract class CommandViewModel : RoutableViewModel, ICommandViewModel, IControlsAppBar
    {
        protected string _TopTitle;
        public string TopTitle { get { return _TopTitle; } protected set { this.RaiseAndSetIfChanged(ref _TopTitle, value); } }

        protected string _Title;
        public string Title { get { return _Title; } protected set { this.RaiseAndSetIfChanged(ref _Title, value); } }

        public CommandViewModel(IGSApp app)
            : base(app)
        { }

        protected ReactiveList<ButtonViewModel> _AppBarButtons;
        public ReactiveList<ButtonViewModel> AppBarButtons
        {
            get
            {
                if (_AppBarButtons == null)
                    _AppBarButtons = new ReactiveList<ButtonViewModel>()
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
        public ReactiveCommand AddCommand
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


    public interface IPlantActionViewModel : ICommandViewModel
    {
        string WeekDay { get; }
        string Date { get; }
        string Time { get; }
        string Note { get; }

        Uri IconUri { get; }

    }

    public interface IPlantCommentViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantMeasureViewModel : IPlantActionViewModel
    {

        MeasurementTypeViewModel Series { get; }
        double? Value { get; }
    }

    public interface IPlantWaterViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantFertilizeViewModel : IPlantActionViewModel
    {


    }

    public interface IPlantPhotographViewModel : IPlantActionViewModel
    {

        Photo PhotoData { get; }
    }

    public abstract class PlantActionViewModel : CommandViewModel, IPlantActionViewModel
    {
        //protected PlantState _State;
        //public PlantState State
        //{
        //    get { return _State; }
        //    protected set
        //    {
        //        if (value == null || value == _State)
        //            return;
        //        this._State = value;

        //        if (_State.Species != null)
        //        {
        //            this.PlantTitle = string.Format("{0} ({1})", _State.Name.ToUpper(), _State.Species.ToUpper());
        //        }
        //        else
        //        {
        //            this.PlantTitle = _State.Name.ToUpper();
        //        }
        //    }
        //}

        protected readonly ActionBase State;

        public string WeekDay { get; protected set; }
        public string Date { get; protected set; }
        public string Time { get; protected set; }


        public Uri IconUri { get { return this.App.BigIconUri[this._IconType]; } }

        protected IconType _IconType;





        public PlantActionViewModel(ActionBase state, IGSApp app)
            : base(app)
        {

            this.State = state;
            if (state != null)
            {
                this.Note = state.Note;
                this.WeekDay = state.Created.ToString("dddd");
                this.Date = state.Created.ToString("d");
                this.Time = state.Created.ToString("t");

            }

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


    public sealed class AddMeasurementViewModelDesign
    {
        public string Title { get { return "measurement"; } }
        public string PlantTitle { get { return "JARI (ALOE VERA)"; } }
        public string Note { get { return "Ah a comment"; } }

        public AddMeasurementViewModelDesign()
        {
            this.MeasurementTypes = MeasurementTypeViewModel.GetAll(null);

        }


        public IList<MeasurementTypeViewModel> MeasurementTypes { get; set; }
    }

    public sealed class MeasurementTypeViewModel : GSViewModelBase
    {

        private IconType _Icon;

        public MeasurementTypeViewModel(MeasurementType type, string title, IconType icon, IGSApp app)
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

        public static IList<MeasurementTypeViewModel> GetAll(IGSApp app)
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



}
