using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
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
        public IReactiveCommand EditCommand { get; protected set; }
        public IReactiveCommand DeleteCommand { get; protected set; }

        public IconType Icon { get; protected set; }
        

        private int _ActionIndex;
        public int ActionIndex
        {
            get
            {
                return _ActionIndex;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ActionIndex, value);
            }
        }


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
           // {PlantActionType.FBCOMMENTED,"commented"}, // before enabling this it needs to be filtered out from the action list UI
            {PlantActionType.DECEASED,"deceased"},
            {PlantActionType.COMMENTED,"commented"},
            {PlantActionType.BLOOMED,"blooming!"}
        };



        public Guid PlantActionId { get; protected set; }


        private Guid? _PlantId;
        public Guid? PlantId
        {
            get
            {
                return _PlantId;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PlantId, value);
            }
        }

        private Guid? _UserId;
        public Guid? UserId
        {
            get
            {
                return _UserId;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _UserId, value);
            }
        }


        private bool _IsEditEnabled;
        public bool IsEditEnabled
        {
            get
            {
                return _IsEditEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsEditEnabled, value);
            }
        }


        public DateTimeOffset Created { get; protected set; }

        public int Updates { get; set; }


        protected IDisposable TimelineLinesSubscription = Disposable.Empty;
        protected IDisposable EditCommandSubscription = Disposable.Empty;
        protected IDisposable DeleteCommandSubscription = Disposable.Empty;


        public PlantActionViewModel(PlantActionType type, IGSAppViewModel app, PlantActionState state = null)
            : base(app)
        {
            this.ActionType = type;
            this.Icon = ActionTypeToIcon[type];
            this.Label = ActionTypeToLabel[type];
            this.State = state;
            //AddCommand.Subscribe(x =>
            //{
            //    App.Router.NavigateBack.Execute(null);
            //});

            if (state != null)
            {
                this.Note = state.Note;
                this.TimelineFirstLine = state.Note;
                this.WeekDay = SharedViewHelpers.FormatWeekDay(state.Created);
                this.Date = state.Created.ToString("d");
                this.Time = state.Created.ToString("t");
                this.PlantActionId = state.Id;
                this.Created = state.Created;
                this.ListenTo<PlantActionPropertySet>(state.Id).Subscribe(x =>
                {
                    SetProperty(x);
                });
            }
            else
            {
                var now = DateTimeOffset.Now;
                this.WeekDay = SharedViewHelpers.FormatWeekDay(now);
                this.Date = now.ToString("d");
                this.Time = now.ToString("t");

            }

            TimelineLinesSubscription = this.WhenAnyValue(x => x.Note).Subscribe(x => this.TimelineFirstLine = x);


            this.WhenAnyValue(x => x.UserId).Subscribe(x =>
            {
                this.IsEditEnabled = x.HasValue && x.Value == App.User.Id ? true : false;
            });

            this.EditCommand = new ReactiveCommand(this.WhenAnyValue(x => x.IsEditEnabled));
            EditCommandSubscription = this.EditCommand.Subscribe(_ => this.Navigate(this));

            this.DeleteCommand = new ReactiveCommand(Observable.Return(state != null));
            //DeleteCommandSubscription = this.DeleteCommand.Subscribe(_ => this.Navigate(this));

            string kind = "";
            switch (type)
            {
                case PlantActionType.BLOOMED:
                    kind = "blooming";
                    break;

                case PlantActionType.COMMENTED:
                    kind = "comment";
                    break;

                case PlantActionType.FBCOMMENTED:
                    kind = "fbComment";
                    break;

                case PlantActionType.DECEASED:
                    kind = "deceased";
                    break;
                
                case PlantActionType.FERTILIZED:
                    kind = "fertilizing";
                    break;

                case PlantActionType.HARVESTED:
                    kind = "harvesting";
                    break;

                case PlantActionType.MEASURED:
                    kind = "measurement";
                    break;

                case PlantActionType.MISTED:
                    kind = "misting";
                    break;

                case PlantActionType.PHOTOGRAPHED:
                    kind = "photo";
                    break;

                case PlantActionType.POLLINATED:
                    kind = "pollination";
                    break;

                case PlantActionType.PRUNED:
                    kind = "pruning";
                    break;

                case PlantActionType.SPROUTED:
                    kind = "sprouting";
                    break;

                case PlantActionType.TRANSFERRED:
                    kind = "transfer";
                    break;

                case PlantActionType.WATERED:
                    kind = "watering";
                    break;
            }

            this.DeleteCommand
               .RegisterAsyncTask((_) => App.HandleCommand(new DeleteAggregate(this.PlantActionId, kind)))
               .Publish()
               .Connect();
        }



        private ReactiveCommand _AddCommand;
        public override IReactiveCommand AddCommand
        {
            get
            {

                if (_AddCommand == null)
                {
                    _AddCommand = new ReactiveCommand(this.CanExecute == null ? Observable.Return(true) : this.CanExecute, false);
                    _AddCommand.Subscribe(this.AddCommandSubscription);
                    this.AsyncAddObservable = _AddCommand.RegisterAsyncTask(AsyncAddCommand);
                    this.AsyncAddObservable.Publish().Connect();

                }
                return _AddCommand;

            }
        }


        protected virtual Task<IGSAggregate> AsyncAddCommand(object _)
        {

            if (State == null)
            {
                return App.HandleCommand(new CreatePlantAction(
                                 Guid.NewGuid(),
                                 this.UserId.Value,
                                 this.PlantId.Value,
                                 this.ActionType,
                                 this.Note
                             )
                {
                    Photo = this.Photo,
                    MeasurementType = this.MeasurementType,
                    Value = this.Value
                });

            }
            else
            {
                return App.HandleCommand(new SetPlantActionProperty(this.PlantActionId)
                {
                    Note = this.Note,
                    Photo = this.Photo,
                    Value = this.Value,
                    MeasurementType = this.MeasurementType
                });
            }

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




        public IObservable<IGSAggregate> AsyncAddObservable { get; protected set; }
    }



    public class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {


        private IPlantMeasureViewModel _PreviousMeasurement;
        public IPlantMeasureViewModel PreviousMeasurement
        {
            get
            {
                return _PreviousMeasurement;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _PreviousMeasurement, value);
            }
        }


        public IList<MeasurementTypeHelper> _Options;
        public IList<MeasurementTypeHelper> Options
        {
            get { return _Options ?? (_Options = MeasurementTypeHelper.Options.Values.ToArray()); }
        }


        readonly ObservableAsPropertyHelper<MeasurementTypeHelper> _SelectedMeasurementType;
        public MeasurementTypeHelper SelectedMeasurementType
        {
            get
            {
                return _SelectedMeasurementType.Value;
            }
        }



        private object _SelectedItem;
        public object SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedItem, value);
            }
        }


        protected string _SValue;
        public string SValue
        {
            get
            {
                return _SValue;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SValue, value);
            }
        }



        public override void AddCommandSubscription(object p)
        {


        }


        //public override IObservable<bool> CanExecute { get; protected set; }

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

            double dValue = 0;
            this.WhenAnyValue(x => x.SValue, x => x)
                .Where(x => double.TryParse(x, out dValue))
                .Subscribe(x => this.Value = dValue);


            this.CanExecute = this.WhenAnyValue(x => x.Value, x => x.MeasurementType, (x, y) => Tuple.Create(x, y))
              .Select(x =>
              {
                  if (state != null && x.Item2 == state.MeasurementType)
                  {
                      return x.Item1.HasValue && x.Item1 != state.Value;
                  }
                  return x.Item1.HasValue && x.Item2 != MeasurementType.NOTYPE;
              });


            var defaultMeasurementType = MeasurementTypeHelper.Options[state != null ? state.MeasurementType : MeasurementType.LENGTH];

            this.WhenAnyValue(x => x.SelectedItem)
                .Select(x =>
                {
                    return x as MeasurementTypeHelper;
                })
                .ToProperty(this, x => x.SelectedMeasurementType, out _SelectedMeasurementType, defaultMeasurementType);


            this.WhenAnyValue(x => x.SelectedMeasurementType).Where(x => x != null).Subscribe(x =>
            {
                this.MeasurementType = x.Type;
            });

            // dispose of the default content
            TimelineLinesSubscription.Dispose();

            this.WhenAnyValue(x => x.SelectedMeasurementType).Subscribe(x => 
            {
                this.TimelineFirstLine = x == null ? null : x.TimelineTitle;
                this.UpdateFirstTimeText();
            });

            this.WhenAnyValue(x => x.Value).Where(x => x.HasValue).Subscribe(x => 
            {
                this.TimelineSecondLine = this.SelectedMeasurementType.FormatValue(x.Value, true);
                UpdateTrendInfos();
            });


            this.SelectedItem = defaultMeasurementType;
            this.SValue = state != null ? defaultMeasurementType.FormatValue(state.Value.Value) : string.Empty;

            UpdateTrendIcon();
            UpdateFirstTimeText();
        }


        public void UpdateTrendInfos()
        {
            UpdateTrendIcon();
            UpdateChangePercentage();
        }


        private void UpdateTrendIcon()
        {
            /*
            if (PreviousMeasurement == null) {
                TrendIcon = null;
            
            } else if (PreviousMeasurement.Value > Value) {
                TrendIcon = IconType.ARROW_DOWN;

            } else if (PreviousMeasurement.Value < Value) {
                TrendIcon = IconType.ARROW_UP;

            } else {
                TrendIcon = IconType.ARROW_RIGHT;

            }
             */
            TrendIcon = IconType.ARROW_DOWN;
        }


        private IconType? _TrendIcon = null;
        public IconType? TrendIcon
        {
            get
            {
                return _TrendIcon;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _TrendIcon, value);
            }
        }


        public void UpdateFirstTimeText()
        {
            if (this.MeasurementType == null)
            {
                FirstTimeText = "";
                return;
            }

            if (MeasurementTypeHelper.Options[this.MeasurementType] == null)
            {
                FirstTimeText = "";
                return;
            }

            FirstTimeText 
                = "first time you measured " 
                + MeasurementTypeHelper.Options[this.MeasurementType].TimelineTitle;
        }


        private string _FirstTimeText = null;
        public string FirstTimeText
        {
            get {
                return _FirstTimeText;
            }

            set {
                this.RaiseAndSetIfChanged(ref _FirstTimeText, value);
            }
        }


        private void UpdateChangePercentage()
        {
            if (PreviousMeasurement == null)
            {
                return;
            }

            double? pct = Value / PreviousMeasurement.Value * 100.0;
            pct = 24.5;

            if (pct > 0)
            {
                ChangePercentage = "+" + string.Format("#.#", pct);
            
            } else {
                ChangePercentage = string.Format("#.#", pct);
            }
        }


        private string _ChangePercentage;
        public string ChangePercentage
        {
            get
            {
                return ChangePercentage;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _ChangePercentage, value);
            }
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

        //public override IObservable<bool> CanExecute { get; protected set; }

        public PlantPhotographViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(PlantActionType.PHOTOGRAPHED, app, state)
        {

            if (state != null && state.Type != PlantActionType.PHOTOGRAPHED)
                throw new InvalidCastException();
            this.Icon = IconType.PHOTO;

            this.CanExecute = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Photo),
                this.WhenAnyValue(x => x.Note),
                (x, y) => state != null ? (x != null && (x != state.Photo || y != state.Note)) : x != null
            );

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


        protected override async Task<IGSAggregate> AsyncAddCommand(object _)
        {
            var action = await base.AsyncAddCommand(_);
            if (State == null)
            {
                await App.HandleCommand(new SchedulePhotoUpload(this.Photo, action.Id));
            }
            return action;
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
