
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Growthstories.Domain.Entities;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{



    public class PlantActionViewModel : DesignViewModelBase, IPlantActionViewModel
    {

        public PlantActionViewModel()
            : this(PlantActionType.WATERED, DateTimeOffset.Now)
        {

        }

        public PlantActionViewModel(PlantActionType type, DateTimeOffset Created)
        {

            this.ActionType = type;
            this.Note = "Just a note";
            this.WeekDay = Created.ToString("dddd");
            this.Date = Created.ToString("d");
            this.Time = Created.ToString("t");
            this.PlantActionId = Guid.NewGuid();
            this.Created = Created;

            _OpenZoomView = new MockReactiveCommand(o =>
            {
                this.IsZoomViewOpen = true;
            });

            this.Icon = ActionTypeToIcon[type];
            this.Label = ActionTypeToLabel[type];
            this.TimelineFirstLine = this.Note;


        }

        public string WeekDay { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        protected string _Note;
        public string Note
        {
            get { return _Note; }
            set
            {
                if (_Note != value)
                {
                    _Note = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected string _TimelineSecondLine;
        public string TimelineSecondLine
        {
            get { return _TimelineSecondLine; }
            set
            {
                if (_TimelineSecondLine != value)
                {
                    _TimelineSecondLine = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected string _TimelineFirstLine;
        public string TimelineFirstLine
        {
            get { return _TimelineFirstLine; }
            set
            {
                if (_TimelineFirstLine != value)
                {
                    _TimelineFirstLine = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PlantActionType ActionType { get; protected set; }


        public IconType Icon { get; protected set; }

        protected bool _IsZoomViewOpen = false;
        public bool IsZoomViewOpen
        {
            get { return _IsZoomViewOpen; }
            set
            {
                if (_IsZoomViewOpen != value)
                {
                    _IsZoomViewOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Guid PlantActionId { get; set; }

        public DateTimeOffset Created { get; set; }


        public string Title { get; set; }

        public string PlantTitle
        {
            get { return ""; }
        }


        MockReactiveCommand _OpenZoomView;
        public ReactiveUI.IReactiveCommand OpenZoomView
        {
            get
            {
                return _OpenZoomView;
            }
        }


        public MeasurementType MeasurementType { get; set; }


        public double? _Value;
        public double? Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                this.RaisePropertyChanged();
            }
        }

        public Photo Photo { get; set; }


        public string Label { get; set; }



        public Guid PlantId { get; set; }


        public Guid UserId { get; set; }


        public IReactiveCommand EditCommand { get; set; }



        public PlantActionState State { get; set; }

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
            {PlantActionType.WATERED, "water"},
            {PlantActionType.TRANSFERRED, "transfer"},
            {PlantActionType.SPROUTED, "sprouting"},
            {PlantActionType.PRUNED, "prune"},
            {PlantActionType.POLLINATED, "pollinate"},
            {PlantActionType.PHOTOGRAPHED,"photograph"},
            {PlantActionType.MISTED, "mist"},
            {PlantActionType.MEASURED,"measure"},
            {PlantActionType.HARVESTED,"harvest"},
            {PlantActionType.FERTILIZED,"fertilize"},
            {PlantActionType.FBCOMMENTED,"comment"},
            {PlantActionType.DECEASED,"declare deceased"},
            {PlantActionType.COMMENTED,"comment"},
            {PlantActionType.BLOOMED,"blooming"}
        };


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
            set { this._SelectedMeasurementType = value; this.RaisePropertyChanged(); }
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
            set { this._SValue = value; this.RaisePropertyChanged(); }
        }

        public PlantMeasureViewModel(DateTimeOffset created)
            : base(PlantActionType.MEASURED, created)
        {
            this.SelectedMeasurementType = MeasurementTypeHelper.Options[MeasurementType.LENGTH];

            this.Note = "Measurement note";
            this.SValue = "34234";
            this.Value = 23423;

            this.TimelineFirstLine = SelectedMeasurementType.TimelineTitle;
            this.TimelineSecondLine = SelectedMeasurementType.FormatValue(this.Value.Value, true);
        }

        public PlantMeasureViewModel()
            : this(DateTimeOffset.Now)
        {

            //this.WhenAnyValue(x => x.SelectedMeasurementType).Subscribe(x => this.MeasurementType = x.Type);


            //double dValue = 0;
            //this.WhenAnyValue(x => x.SValue, x => x)
            //    .Where(x => double.TryParse(x, out dValue))
            //    .Subscribe(x => this.Value = dValue);

            //this.CanExecute = this.WhenAnyValue(x => x.Value, x => x.HasValue);

            //if (state != null)
            //{
            //    this.SelectedMeasurementType = Options[state.MeasurementType];
            //    this.SValue = state.Value.Value.ToString("F1");
            //    this.Value = state.Value;
            //}

        }

        //public override void SetProperty(PlantActionPropertySet prop)
        //{
        //    base.SetProperty(prop);
        //    this.Value = prop.Value;
        //    this.SValue = prop.Value.Value.ToString("F1");

        //}

    }




    public class PlantPhotographViewModel : PlantActionViewModel, IPlantPhotographViewModel
    {

        public PlantPhotographViewModel()
            : this(@"/TestData/517e100d782a828894.jpg", DateTimeOffset.Now)
        {

        }

        public PlantPhotographViewModel(string photo, DateTimeOffset created)
            : base(PlantActionType.PHOTOGRAPHED, created)
        {
            if (photo != null)
            {
                this.Photo = new Photo()
                {
                    LocalFullPath = photo,
                    LocalUri = photo
                };
            }



        }





        public IReactiveCommand PhotoTimelineTap
        {
            get { return new MockReactiveCommand(); }
        }


        public IReactiveCommand PhotoChooserCommand
        {
            get { return new MockReactiveCommand(); }
        }
    }




}
