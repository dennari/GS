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


        private bool _OwnAction;
        public bool OwnAction
        {
            get
            {
                return _OwnAction;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _OwnAction, value);
            }

        }


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
                this.Created = now;

                // I wonder if setting Created here is a problem,
                // for some reason it has been avoided before
                //   -- JOJ 16.1.2014
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



        //
        // Not exactly elegant to have this here
        //
        // Best option would be to populate context menu items
        // based completely dynamically, but will not bother
        // to refactor now.
        //
        //  -- JOJ 17.1.2014
        //
        private bool _ShowSetAsProfilePicture;
        public bool ShowSetAsProfilePicture
        {
            get
            {
                return _ShowSetAsProfilePicture;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ShowSetAsProfilePicture, value);
            }
        }

        public void UpdateShowSetAsProfilePicture()
        {
            var vm = this as IPlantPhotographViewModel;
            if (vm == null)
            {
                ShowSetAsProfilePicture = false;

            }
            else
            {
                ShowSetAsProfilePicture = !vm.IsProfilePhoto;

            }
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
                    var obs = _AddCommand.RegisterAsyncTask(AsyncAddCommand).Publish();
                    this.AsyncAddObservable = obs.Select(_ => this);
                    obs.Connect();

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
                    Note = this.Note == State.Note ? null : this.Note,
                    Photo = this.Photo == State.Photo ? null : this.Photo,
                    Value = this.Value == State.Value ? null : this.Value,
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




        public IObservable<IPlantActionViewModel> AsyncAddObservable { get; protected set; }
    }



    public class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {


        private IReactiveDerivedList<IPlantMeasureViewModel> _MeasurementActions;
        public IReactiveDerivedList<IPlantMeasureViewModel> MeasurementActions
        {
            get
            {
                return _MeasurementActions;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _MeasurementActions, value);
            }
        }


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


        public override void AddCommandSubscription(object p) { }


        //public override IObservable<bool> CanExecute { get; protected set; }

        public PlantMeasureViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(PlantActionType.MEASURED, app, state)
        {
            this.Log().Info("measureviewmodel for {0}", state.Id);
            if (state.Value == null)
            {
                this.Log().Info("value is null");
            }
            
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
            PreviousMeasurement = null;

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
                UpdatePreviousMeasurement();
                UpdateCountText(); // is not necessarily called by UpdatePreviousMeasurement
            });

            this.WhenAnyValue(x => x.Value).Where(x => x.HasValue).Subscribe(x =>
            {
                this.TimelineSecondLine = this.SelectedMeasurementType.FormatValue(x.Value, true);
            });

            this.WhenAnyValue(x => x.PreviousMeasurement).Subscribe(_ =>
            {
                UpdateTrendInfos();
                UpdateCountText();
            });

            this.WhenAnyValue(x => x.Value).Subscribe(x =>
            {
                this.UpdateTrendInfos();
            });

            this.SelectedItem = defaultMeasurementType;
            this.SValue = state != null && state.Value.HasValue ? defaultMeasurementType.FormatValue(state.Value.Value) : string.Empty;

            UpdateCountText();
        }


        // Update the trend info data based on 
        // the current _PreviousMeasurement
        //
        public void UpdateTrendInfos()
        {
            UpdateShowTrendInfos();
            UpdateTrendIcon();
            UpdateChangePercentage();
        }


        // Update the previous measurement
        //
        // This needs to be updated whenever one of the previous 
        // measurements is deleted or modified. Alternatively we
        // can just call this whenever the view is entered, which
        // is how we are doing it for now.
        //
        //  -- JOJ 15.1.2014
        public void UpdatePreviousMeasurement()
        {
            if (MeasurementActions != null)
            {
                var actions = MeasurementActions
                    .Where(x => x.Created < this.Created && x.MeasurementType == MeasurementType)
                    .OrderByDescending(x => x.Created);

                CountForType = actions.Count();

                if (actions.Count() > 0)
                {
                    PreviousMeasurement = actions.First();

                }
                else
                {
                    PreviousMeasurement = null;

                }
            }
        }


        private void UpdateTrendIcon()
        {

            if (PreviousMeasurement == null)
            {
                TrendIcon = null;

            }
            else if (PreviousMeasurement.Value > Value)
            {
                TrendIcon = IconType.ARROW_DOWN;

            }
            else if (PreviousMeasurement.Value < Value)
            {
                TrendIcon = IconType.ARROW_UP;

            }
            else
            {
                TrendIcon = IconType.ARROW_RIGHT;

            }
        }


        private bool _ShowTrendInfos;
        public bool ShowTrendInfos
        {
            get
            {
                return _ShowTrendInfos;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ShowTrendInfos, value);
            }
        }


        private void UpdateShowTrendInfos()
        {
            if (PreviousMeasurement == null || !Value.HasValue)
            {
                ShowTrendInfos = false;
                return;
            }
            ShowTrendInfos = true;
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


        private int _CountForType;
        public int CountForType
        {
            get
            {
                return _CountForType;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _CountForType, value);
            }

        }


        public void UpdateCountText()
        {
            if (!MeasurementTypeHelper.Options.ContainsKey(this.MeasurementType))
            {
                //this.Log().Warn("BUG: MeasurementType is null");
                CountText = "";
                return;
            }

            CountText
                = Ordinal(CountForType + 1)
                + " time you measured "
                + MeasurementTypeHelper.Options[this.MeasurementType].TimelineTitle;
        }


        public static string Ordinal(int number)
        {
            const string TH = "th";
            var s = number.ToString();

            number %= 100;

            if ((number >= 11) && (number <= 13))
            {
                return s + TH;
            }

            switch (number % 10)
            {
                case 1:
                    return s + "st";
                case 2:
                    return s + "nd";
                case 3:
                    return s + "rd";
                default:
                    return s + TH;
            }
        }


        private string _CountText = null;
        public string CountText
        {
            get
            {
                return _CountText;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _CountText, value);
            }
        }


        private void UpdateChangePercentage()
        {
            if (PreviousMeasurement == null || !Value.HasValue || !PreviousMeasurement.Value.HasValue)
            {
                return;
            }

            double? pct = Value / PreviousMeasurement.Value * 100.0 - 100.0;

            if (pct > 0)
            {
                ChangePercentage = "+" + string.Format("{0:F1}", pct) + "%";

            }
            else
            {
                ChangePercentage = string.Format("{0:F1}", pct) + "%";
            }
        }


        private string _ChangePercentage;
        public string ChangePercentage
        {
            get
            {
                return _ChangePercentage;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref _ChangePercentage, value);
            }
        }


        public override void SetProperty(PlantActionPropertySet prop)
        {
            base.SetProperty(prop);
            if (prop.Value != null && prop.Value.HasValue)
            {
                this.Value = prop.Value;
                this.SValue = prop.Value.Value.ToString("F1");
            }
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

        private bool _IsProfilePhoto;
        public bool IsProfilePhoto
        {
            get
            {
                return _IsProfilePhoto;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsProfilePhoto, value);
            }
        }

        // Notify the viewmodel that the UI failed to
        // download an image
        //
        public void NotifyImageDownloadFailed()
        {
            if (!OwnAction)
            {
                App.NotifyImageDownloadFailed();
            }
            else
            {
                this.Log().Warn("image open failed for non-followed user");
            }
        }


        private bool _CanChooseNewPhoto;
        public bool CanChooseNewPhoto
        {
            get
            {
                return _CanChooseNewPhoto;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _CanChooseNewPhoto, value);
            }
        }


        public IReactiveCommand SetAsProfilePictureCommand { get; protected set; }

        //public override IObservable<bool> CanExecute { get; protected set; }

        public PlantPhotographViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(PlantActionType.PHOTOGRAPHED, app, state)
        {

            CanChooseNewPhoto = true;

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
                CanChooseNewPhoto = false;
            }

            this.OpenZoomView.Subscribe(x =>
                {
                    this.Log().Info("openzoomview");
                    this.IsZoomViewOpen = !this.IsZoomViewOpen;
                }
            );

            this.PhotoTimelineTap = new ReactiveCommand();
            this.PhotoChooserCommand = new ReactiveCommand();
            this.SetAsProfilePictureCommand = new ReactiveCommand();

            this.WhenAnyValue(z => z.PlantId).Subscribe(u =>
            {
                if (u != null)
                {
                    this.ListenTo<ProfilepictureSet>((Guid)u).Subscribe(x =>
                    {
                        IsProfilePhoto = this.PlantActionId == x.PlantActionId;
                    });
                }
            });


            this.SetAsProfilePictureCommand.Subscribe(_ =>
            {
                App.HandleCommand(new SetProfilepicture((Guid)PlantId, this.Photo, PlantActionId));
            });

            this.WhenAnyValue(x => x.IsProfilePhoto).Subscribe(_ =>
            {
                UpdateShowSetAsProfilePicture();
            });

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
