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


    public class TimelineActionViewModel : GSViewModelBase, ITimelineActionViewModel
    {
        private readonly IPlantActionViewModel Vm;


        public TimelineActionViewModel(IPlantActionViewModel vm)
            : base(vm.App)
        {
            this.Vm = vm;
        }


        public string WeekDay { get { return Vm.WeekDay; } }

        public string Date { get { return Vm.Date; } }

        public string Time { get { return Vm.Time; } }

        public string Note { get { return Vm.Note; } }

        public string Label { get { return Vm.Label; } }

        public PlantActionType ActionType { get { return Vm.ActionType; } }

        public IconType Icon { get { return Vm.Icon; } }

        public Guid PlantActionId { get { return Vm.PlantActionId; } }

        public DateTimeOffset Created { get { return Vm.Created; } }

        public MeasurementType MeasurementType { get { return Vm.MeasurementType; } }

        public double? Value { get { return Vm.Value; } }

        public IReactiveCommand EditCommand { get; set; }

        public Photo Photo { get { return Vm.Photo; } }

        public PlantActionState State { get { return Vm.State; } }
    }


    public sealed class MeasurementTypeViewModel : GSViewModelBase
    {


        public MeasurementTypeViewModel(MeasurementType type, string title, IconType icon, IGSAppViewModel app)
            : base(app)
        {
            this.Type = type;
            this.Title = title;
            this.IconType = icon;


        }

        public MeasurementType Type { get; set; }

        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }

        public IconType IconType { get; private set; }

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




    public class PlantMeasureViewModel : PlantActionViewModel, IPlantMeasureViewModel
    {



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



        public override void AddCommandSubscription(object p)
        {


        }


        public PlantMeasureViewModel(IGSAppViewModel app, PlantActionState state = null)
            : base(PlantActionType.MEASURED, app, state)
        {
            if (state != null && state.Type != PlantActionType.MEASURED)
                throw new InvalidOperationException();

            this.MeasurementTypes = MeasurementTypeViewModel.GetAll(app);
            this.SeriesSelected = new ReactiveCommand();
            this.SeriesSelected
                .OfType<MeasurementTypeViewModel>()
                .ToProperty(this, x => x.Series, out _Series, state != null ? MeasurementTypes.FirstOrDefault(x => x.Type == state.MeasurementType) : MeasurementTypes[0]);


            this.WhenAny(x => x.Series, x => x.GetValue()).Subscribe(x => this.MeasurementType = x.Type);


            double dValue = 0;
            this.WhenAnyValue(x => x.SValue, x => x)
                .Where(x => double.TryParse(x, out dValue))
                .Subscribe(x => this.Value = dValue);

            this.CanExecute = this.WhenAnyValue(x => x.Value, x => x.HasValue);

            if (state != null)
            {
                this.MeasurementType = state.MeasurementType;
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


    }

}
