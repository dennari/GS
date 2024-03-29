﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.UI.Services;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{



    public class ScheduleViewModel : CommandViewModel, IScheduleViewModel
    {
        public ScheduleType Type { get; private set; }
        private readonly ScheduleState ScheduleState;
        public Guid? Id { get; private set; }

        protected ReactiveCommand _OtherScheduleSelected = new ReactiveCommand();
        public ReactiveCommand OtherScheduleSelected { get { return _OtherScheduleSelected; } }


        public ScheduleViewModel(ScheduleState state, ScheduleType scheduleType, IGSAppViewModel app)
            : base(app)
        {
            // TODO: Complete member initialization
            this.ScheduleState = state;
            this.Type = scheduleType;
            this.Title = scheduleType == ScheduleType.WATERING ? "watering schedule" : "nourishing schedule";


            this.CanExecute = this.WhenAnyValue(x => x.Interval).Select(x => x != null);
            //.Select(x => !string.IsNullOrWhiteSpace(x) && double.TryParse(x, out dVal) && (state == null || this.ValueType.Compute(x) != state.Interval));
            this.WhenAny(x => x.OtherSchedules, x => x.GetValue()).Subscribe(x =>
            {
                if (x == null)
                {
                    this.HasOtherSchedules = false;
                    return;
                }
                this.HasOtherSchedules = x.Count() > 0 ? true : false;

            });

            OtherScheduleSelected.OfType<Tuple<IPlantViewModel, IScheduleViewModel>>().Subscribe(x =>
            {
                this.Interval = x.Item2.Interval;
            });


            TimeSpan? originalInterval = TimeSpan.FromSeconds(state != null ? state.Interval : 24 * 3600 * 2);


            this.WhenAnyValue(x => x.Interval)
                .Do(x => IntervalLabel = ComputeIntervalLabel(x))
                .Select(x => x != originalInterval)
                .ToProperty(this, x => x.HasChanged, out _HasChanged, false);


            //this.WhenAny(x => x.IsEnabled, x => x.GetValue()).Subscribe(x =>
            //{
            //    if (!x && ScheduleState != null)
            //        this.Id = null;

            //});

            //this.WhenAny(x => x.OtherSchedules, x => x.GetValue()).Where(x => x != null).Select(x => x.)
            this.Interval = originalInterval;
            if (state != null)
                this.Id = state.Id;
        }



        protected TimeSpan? _Interval;
        public TimeSpan? Interval
        {
            get
            {
                return _Interval;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Interval, value);
            }
        }


        private ObservableAsPropertyHelper<bool> _HasChanged;
        public bool HasChanged
        {
            get
            {
                return _HasChanged.Value;
            }
        }


        protected string ComputeIntervalLabel(TimeSpan? tt)
        {
            if (!tt.HasValue)
                return null;

            return PlantScheduler.IntervalText(tt.Value);
        }


        private string _IntervalLabel;
        public string IntervalLabel
        {
            get
            {
                return _IntervalLabel;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IntervalLabel, value);
            }
        }



        public IconType OKIcon
        {
            get
            {
                return IconType.CHECK;
            }
        }

        public IconType CancelIcon
        {
            get
            {
                return IconType.CANCEL;
            }
        }

        public string ScheduleTypeLabel
        {
            get
            {
                return string.Format("{0}", this.Type == ScheduleType.WATERING ? "watering schedule" : "nourishing schedule");
            }
        }


        // this is only used to help bindings in the switch datatemplate in AddEditPlantView.xaml
        public ObservableAsPropertyHelper<bool> _IsEnabled;
        public bool IsEnabled
        {
            get
            {
                if (_IsEnabled == null)
                    return false;
                return _IsEnabled.Value;
            }
        }







        protected IReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>> _OtherSchedules;
        public IReactiveList<Tuple<IPlantViewModel, IScheduleViewModel>> OtherSchedules
        {

            get
            {
                return _OtherSchedules;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _OtherSchedules, value);
            }
        }

        protected object _SelectedCopySchedule;
        public object SelectedCopySchedule
        {
            get
            {
                return _SelectedCopySchedule;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedCopySchedule, value);
            }
        }

        protected bool _HasOtherSchedules;
        public bool HasOtherSchedules
        {
            get
            {
                return _HasOtherSchedules;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _HasOtherSchedules, value);
            }
        }



        public override void AddCommandSubscription(object p)
        {

            this.App.Router.NavigateBack.Execute(null);

        }

        public async Task<Schedule> Create()
        {

            var id = Guid.NewGuid();
            this.Id = id;
            return (Schedule)(await this.App.HandleCommand(new CreateSchedule(id, (long)Interval.Value.TotalSeconds)));

        }



    }
}
