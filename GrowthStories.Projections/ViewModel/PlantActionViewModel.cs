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


    public class PlantActionViewModel : CommandViewModel, IPlantActionViewModel
    {

        public PlantActionState State { get; protected set; }
        public string Label { get; protected set; }
        public string WeekDay { get; protected set; }
        public string Date { get; protected set; }
        public string Time { get; protected set; }
        public PlantActionType ActionType { get; protected set; }
        public IReactiveCommand EditCommand { get; set; }

        public IconType Icon { get; protected set; }


        public static readonly Dictionary<PlantActionType, IconType> ActionTypeToIcon = new Dictionary<PlantActionType, IconType>()
        {
            {PlantActionType.WATERED, IconType.WATER},
            {PlantActionType.TRANSFERRED, IconType.CHANGESOIL},
            {PlantActionType.SPROUTED, IconType.SPROUTING},
            {PlantActionType.PRUNED, IconType.PRUNING},
            {PlantActionType.POLLINATED, IconType.POLLINATION},
            {PlantActionType.PHOTOGRAPHED,IconType.PHOTO},
            {PlantActionType.MISTED, IconType.MISTING},
            {PlantActionType.MEASURED,IconType.MEASURE},
            {PlantActionType.HARVESTED,IconType.HARVESTING},
            {PlantActionType.FERTILIZED,IconType.FERTILIZE},
            {PlantActionType.FBCOMMENTED,IconType.NOTE},
            {PlantActionType.DECEASED,IconType.DECEASED},
            {PlantActionType.COMMENTED,IconType.NOTE},
            {PlantActionType.BLOOMED,IconType.BLOOMING}
        };

        public static readonly PlantActionType[] ActionTypes = ActionTypeToIcon.Keys.ToArray();
        public static readonly PlantActionType[] NonGenericActionTypes = new PlantActionType[] { PlantActionType.MEASURED, PlantActionType.PHOTOGRAPHED };

        public static readonly Dictionary<PlantActionType, string> ActionTypeToLabel = new Dictionary<PlantActionType, string>()
        {
            {PlantActionType.WATERED, "watered"},
            {PlantActionType.TRANSFERRED, "transferred"},
            {PlantActionType.SPROUTED, "sprouting!"},
            {PlantActionType.PRUNED, "pruned"},
            {PlantActionType.POLLINATED, "pollinated"},
            {PlantActionType.PHOTOGRAPHED,"photographed"},
            {PlantActionType.MISTED, "misted"},
            {PlantActionType.MEASURED,"measured"},
            {PlantActionType.HARVESTED,"harvested"},
            {PlantActionType.FERTILIZED,"nourished"},
            {PlantActionType.FBCOMMENTED,"commented"},
            {PlantActionType.DECEASED,"declare deceased"},
            {PlantActionType.COMMENTED,"commented"},
            {PlantActionType.BLOOMED,"blooming!"}
        };



        public Guid PlantActionId { get; protected set; }
        public Guid PlantId { get; set; }
        public Guid UserId { get; set; }


        public DateTimeOffset Created { get; protected set; }

        public int Updates { get; set; }

        public PlantActionViewModel(PlantActionType type, IGSAppViewModel app, PlantActionState state = null)
            : base(app)
        {
            this.ActionType = type;
            this.Icon = ActionTypeToIcon[type];
            this.Label = ActionTypeToLabel[type];
            this.State = state;

            if (state != null)
            {
                this.Note = state.Note;
                this.TimelineFirstLine = state.Note;
                this.WeekDay = state.Created.ToString("dddd");
                this.Date = state.Created.ToString("d");
                this.Time = state.Created.ToString("t");
                this.PlantActionId = state.Id;
                this.Created = state.Created;
                this.ListenTo<PlantActionPropertySet>(state.Id).Subscribe(x =>
                {
                    SetProperty(x);
                });

                var updatePipe = this.AddCommand.RegisterAsyncTask(_ =>
                {

                    return App.HandleCommand(new SetPlantActionProperty(this.PlantActionId)
                    {
                        Note = this.Note,
                        Photo = this.Photo,
                        Value = this.Value,
                        MeasurementType = this.MeasurementType
                    });

                });


                updatePipe.Subscribe(x => Updates++);

            }
            else
            {

                this.AddCommand.RegisterAsyncTask(_ =>
                {


                    return App.HandleCommand(new CreatePlantAction(
                                   Guid.NewGuid(),
                                   this.UserId,
                                   this.PlantId,
                                   this.ActionType,
                                   this.Note
                               )
                    {
                        Photo = this.Photo,
                        MeasurementType = this.MeasurementType,
                        Value = this.Value
                    });


                });

            }

            TimelineLinesSetup();

        }

        protected virtual void TimelineLinesSetup()
        {
            this.WhenAnyValue(x => x.Note).Subscribe(x => this.TimelineFirstLine = x);

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

        protected string _TimelineFirstLine;
        public string TimelineFirstLine
        {
            get
            {
                return _TimelineFirstLine;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _TimelineFirstLine, value);
            }
        }

        protected string _TimelineSecondLine;
        public string TimelineSecondLine
        {
            get
            {
                return _TimelineSecondLine;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _TimelineSecondLine, value);
            }
        }

        protected MeasurementType _MeasurementType;
        public MeasurementType MeasurementType
        {
            get
            {
                return _MeasurementType;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _MeasurementType, value);
            }
        }

        protected double? _Value;
        public double? Value
        {
            get
            {
                return _Value;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Value, value);
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




        IReactiveCommand _OpenZoomView = new ReactiveCommand();
        public IReactiveCommand OpenZoomView
        {
            get { return _OpenZoomView; }
        }

        public override void AddCommandSubscription(object p)
        {
            base.AddCommandSubscription(p);
        }



    }



    public class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {

        public IList<MeasurementTypeHelper> _Options;
        public IList<MeasurementTypeHelper> Options
        {
            get { return _Options ?? (_Options = MeasurementTypeHelper.Options.Values.ToArray()); }
        }

        //public Dictionary<MeasurementType, MeasurementTypeHelper> Options
        //{
        //    get {return PlantMeasureViewModel}
        //}
        //public IList<MeasurementTypeHelper> MeasurementTypes { get; protected set; }


        protected MeasurementTypeHelper _SelectedMeasurementType;
        public MeasurementTypeHelper SelectedMeasurementType
        {
            get { return _SelectedMeasurementType; }
            set { this.RaiseAndSetIfChanged(ref _SelectedMeasurementType, value); }
        }

        public object SelectedItem
        {
            get
            {
                return SelectedMeasurementType;
            }
            set
            {
                var v = value as MeasurementTypeHelper;
                if (v != null)
                    this.SelectedMeasurementType = v;
            }
        }

        protected string _SValue;
        public string SValue
        {
            get { return _SValue; }
            set { this.RaiseAndSetIfChanged(ref _SValue, value); }
        }




        public override void AddCommandSubscription(object p)
        {


        }


        public PlantMeasureViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(PlantActionType.MEASURED, app, state)
        {
            if (state != null && state.Type != PlantActionType.MEASURED)
                throw new InvalidOperationException();

            //this.MeasurementTypes = MeasurementTypeHelper.GetAll(app);
            //this.SeriesSelected = new ReactiveCommand();
            //this.SeriesSelected
            //    .OfType<MeasurementTypeHelper>()
            //    .ToProperty(this, x => x.Series, out _Series, state != null ? MeasurementTypes.FirstOrDefault(x => x.Type == state.MeasurementType) : MeasurementTypes[0]);


            this.WhenAnyValue(x => x.SelectedMeasurementType).Where(x => x != null).Subscribe(x => this.MeasurementType = x.Type);


            double dValue = 0;
            this.WhenAnyValue(x => x.SValue, x => x)
                .Where(x => double.TryParse(x, out dValue))
                .Subscribe(x => this.Value = dValue);

            this.CanExecute = this.WhenAnyValue(x => x.Value, x => x.MeasurementType, (x, y) => x.HasValue && y != MeasurementType.NOTYPE);

            if (state != null)
            {
                this.SelectedMeasurementType = MeasurementTypeHelper.Options[state.MeasurementType];
                this.SValue = this.SelectedMeasurementType.FormatValue(state.Value.Value);
                this.Value = state.Value;
            }
            else
            {
                this.SelectedMeasurementType = MeasurementTypeHelper.Options[MeasurementType.LENGTH];
            }






        }

        protected override void TimelineLinesSetup()
        {
            this.WhenAnyValue(x => x.SelectedMeasurementType).Subscribe(x => this.TimelineFirstLine = x == null ? null : x.TimelineTitle);
            this.WhenAnyValue(x => x.Value).Where(x => x.HasValue).Subscribe(x => this.TimelineSecondLine = this.SelectedMeasurementType.FormatValue(x.Value, true));
        }

        public override void SetProperty(PlantActionPropertySet prop)
        {
            base.SetProperty(prop);
            this.Value = prop.Value;
            this.SValue = prop.Value.Value.ToString("F1");

        }

    }





    public class PlantPhotographViewModel : PlantActionViewModel, IPlantPhotographViewModel
    {

        bool _IsZoomViewOpen = false;
        public bool IsZoomViewOpen
        {
            get { return _IsZoomViewOpen; }
            set { this.RaiseAndSetIfChanged(ref _IsZoomViewOpen, value); }
        }


        public PlantPhotographViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(PlantActionType.PHOTOGRAPHED, app, state)
        {

            if (state != null && state.Type != PlantActionType.PHOTOGRAPHED)
                throw new InvalidCastException();
            this.Icon = IconType.PHOTO;


            if (state != null)
            {
                this.Photo = state.Photo;
            }

            this.OpenZoomView.Subscribe(x => this.IsZoomViewOpen = !this.IsZoomViewOpen);

            this.PhotoTimelineTap = new ReactiveCommand();
            this.PhotoChooserCommand = new ReactiveCommand();
        }


        public override void AddCommandSubscription(object p)
        {
            //this.SendCommand(new Photograph(this.State.EntityId, this.State.PlantId, this.Note, this.Path), true);
        }


        public override void SetProperty(PlantActionPropertySet prop)
        {
            base.SetProperty(prop);
            this.Photo = prop.Photo;
        }



        public IReactiveCommand PhotoTimelineTap { get; protected set; }
        public IReactiveCommand PhotoChooserCommand { get; protected set; }

    }

}
