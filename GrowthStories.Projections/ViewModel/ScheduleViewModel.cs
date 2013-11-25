using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using System.Reactive.Linq;
using Growthstories.Domain.Entities;
using Growthstories.Core;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{



    public class ScheduleViewModel : CommandViewModel, IScheduleViewModel
    {
        public ScheduleType Type { get; private set; }
        private readonly ScheduleState ScheduleState;


        public ScheduleViewModel(ScheduleState state, ScheduleType scheduleType, IGSAppViewModel app)
            : base(app)
        {
            // TODO: Complete member initialization
            this.ScheduleState = state;
            this.Type = scheduleType;
            this.Title = scheduleType == ScheduleType.WATERING ? "watering schedule" : "nourishing schedule";

            //this.SelectValueType = new ReactiveCommand();
            //this.SelectValueType
            //     .OfType<IntervalValue>()
            //     .ToProperty(this, x => x.ValueType, out this._ValueType, new IntervalValue(IntervalValueType.DAY));

            //this.ValueTypes = IntervalValue.GetAll();

            //double dVal;

            //this.WhenAny(x => x.Id, x => x.Value, (id, val) => Tuple.Create(id.GetValue(), val.GetValue()))
            //    .Where(x => x.Item1 != default(Guid) && double.TryParse(x.Item2, out dVal))
            //    .Select(x => this.format(this.ValueType.Compute(x.Item2), x.Item1))
            //    .ToProperty(this, x => x.Label, out this._Label, state != null ? this.format(state.Interval, state.Id) : "");



            this.CanExecute = this.WhenAny(x => x.Interval, x => x.GetValue()).Select(x => x != null);
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
            this.WhenAny(x => x.SelectedCopySchedule, x => x.GetValue()).Where(x => x != null).OfType<Tuple<IPlantViewModel, IScheduleViewModel>>().Subscribe(x =>
            {
                this.Interval = x.Item2.Interval;
            });

            TimeSpan? originalInterval = ScheduleState != null ? TimeSpan.FromSeconds(ScheduleState.Interval) : (TimeSpan?)null;


            this.WhenAnyValue(x => x.Interval).Subscribe(x =>
            {
                if (x != null)
                {
                    if (originalInterval == null)
                    {
                        HasChanged = true;
                        this.Id = Guid.NewGuid();
                    }
                    else
                    {
                        HasChanged = originalInterval == x ? false : true;

                    }
                    this.IsEnabled = true;
                }
                else
                {
                    this.IsEnabled = false;
                }

                this.raisePropertyChanged("IntervalLabel");
            });

            //this.WhenAny(x => x.IsEnabled, x => x.GetValue()).Subscribe(x =>
            //{
            //    if (!x && ScheduleState != null)
            //        this.Id = null;

            //});

            //this.WhenAny(x => x.OtherSchedules, x => x.GetValue()).Where(x => x != null).Select(x => x.)

            if (ScheduleState != null)
            {
                //this.Value = this.ValueType.Compute(ScheduleState.Interval);

                this.Interval = TimeSpan.FromSeconds(ScheduleState.Interval);
                this.Id = ScheduleState.Id;
            }

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

        protected bool _HasChanged;
        public bool HasChanged
        {
            get
            {
                return _HasChanged;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _HasChanged, value);
            }
        }


        public string IntervalLabel
        {
            get
            {
                if (Interval.HasValue)
                {
                    var t = Interval.Value;
                    //int w = t.TotalWeeks(), h = t.

                    if (t.TotalWeeks() > 0)
                        return string.Format("{0:D} weeks, {1:D} days", t.TotalWeeks(), t.DaysAfterWeeks());
                    if (t.Days > 0)
                        return string.Format("{0:%d} days, {0:%h} hours", t);
                    return string.Format("{0:%h} hours", t);

                }
                return "Not set";
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
                return string.Format("{0}", this.Type == ScheduleType.WATERING ? "watering schedule" : "fertilizing schedule");
            }
        }


        bool _IsEnabled;
        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsEnabled, value);
            }
        }



        public DateTimeOffset ComputeNext(DateTimeOffset last)
        {
            if (Interval == null)
                throw new InvalidOperationException("This schedule is unspecified, so the next occurence cannot be computed.");
            return last + Interval.Value;
        }


        //protected string format(long interval, Guid id)
        //{
        //    return string.Format("{0} {2}", this.ValueType.Compute(interval), id, this.ValueType);
        //}

        protected Guid? _Id;
        public Guid? Id
        {
            get { return _Id; }
            protected set { this.RaiseAndSetIfChanged(ref _Id, value); }
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

        public Task<Schedule> Create()
        {
            //long val = this.ValueType.Compute(this.Value);
            if (!Interval.HasValue || !Id.HasValue)
                return null;
            if (this.ScheduleState != null && Interval.Value == TimeSpan.FromSeconds(this.ScheduleState.Interval) && IsEnabled)
            {

                return null;
            }


            return Task.Run(async () => (Schedule)(await this.App.HandleCommand(new CreateSchedule(Id.Value, (long)Interval.Value.TotalSeconds))));

        }



    }
}
