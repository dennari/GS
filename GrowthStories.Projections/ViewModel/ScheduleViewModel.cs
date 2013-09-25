using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using System.Reactive.Linq;
using Growthstories.Domain.Entities;

namespace Growthstories.UI.ViewModel
{

    public enum IntervalValueType
    {
        HOUR,
        DAY
    }


    public sealed class IntervalValueViewModel : GSViewModelBase
    {
        private readonly IntervalValueType Type;
        public IntervalValueViewModel(IntervalValueType type, IGSApp app)
            : base(app)
        {
            this.Type = type;
        }

        public long Compute(string sValue)
        {
            var dValue = double.Parse(sValue);
            return (long)(dValue * this.Unit);
        }

        public string Compute(long lValue)
        {
            return (lValue / Unit).ToString("F1");
        }

        public long Unit
        {
            get
            {
                return this.Type == IntervalValueType.DAY ? 24 * 3600 : 3600;
            }
        }


        public string Title
        {
            get { return this.Type == IntervalValueType.DAY ? "days" : "hours"; }
        }

        public override string ToString()
        {
            return this.Title;
        }

        public static IList<IntervalValueViewModel> GetAll(IGSApp app)
        {
            return new List<IntervalValueViewModel>()
            {
                new IntervalValueViewModel(IntervalValueType.DAY,app),
                new IntervalValueViewModel(IntervalValueType.HOUR,app)
            };
        }
    }

    public class ScheduleViewModel : PlantActionViewModel
    {
        private ScheduleType scheduleType;
        private readonly ScheduleState ScheduleState;


        public ScheduleViewModel(ScheduleState state, ScheduleType scheduleType, IGSApp app)
            : base(null, app)
        {
            // TODO: Complete member initialization
            this.ScheduleState = state;
            this.scheduleType = scheduleType;
            this.Title = scheduleType == ScheduleType.WATERING ? "watering schedule" : "nourishing schedule";

            this.SelectValueType = new ReactiveCommand();
            this.SelectValueType
                 .OfType<IntervalValueViewModel>()
                 .ToProperty(this, x => x.ValueType, out this._ValueType, new IntervalValueViewModel(IntervalValueType.DAY, app));

            this.ValueTypes = IntervalValueViewModel.GetAll(app);

            double dVal;

            this.WhenAny(x => x.Id, x => x.Value, (id, val) => Tuple.Create(id.GetValue(), val.GetValue()))
                .Where(x => x.Item1 != default(Guid) && double.TryParse(x.Item2, out dVal))
                .Select(x => this.format(this.ValueType.Compute(x.Item2), x.Item1))
                .ToProperty(this, x => x.Label, out this._Label, state != null ? this.format(state.Interval, state.Id) : "Select");

            this.CanExecute = this.WhenAny(x => x.Value, x => x.GetValue())
               .Select(x => !string.IsNullOrWhiteSpace(x) && double.TryParse(x, out dVal) && (state == null || this.ValueType.Compute(x) != state.Interval));

            if (ScheduleState != null)
            {
                this.Value = this.ValueType.Compute(ScheduleState.Interval);
                this.Id = ScheduleState.Id;
            }

        }

        protected string format(long interval, Guid id)
        {
            return string.Format("{0} {2} / {1}", this.ValueType.Compute(interval), id, this.ValueType);
        }

        protected Guid _Id;
        public Guid Id
        {
            get { return _Id; }
            protected set { this.RaiseAndSetIfChanged(ref _Id, value); }
        }

        protected ObservableAsPropertyHelper<string> _Label;
        public string Label
        {
            get
            {
                return _Label.Value;
            }
        }


        public ReactiveCommand SelectValueType { get; protected set; }




        public override void AddCommandSubscription(object p)
        {

            long val = this.ValueType.Compute(this.Value);
            if (this.ScheduleState != null && val == this.ScheduleState.Interval)
            {
                this.App.Router.NavigateBack.Execute(null);
                return;
            }


            this.Id = Guid.NewGuid();


            try
            {

                var cmd = new CreateSchedule(Id, App.Context.CurrentUser.Id, val);
                this.SendCommand(cmd, false);
                this.App.Router.NavigateBack.Execute(null);
            }
            catch
            {

            }



        }


        protected string _Value;
        public string Value
        {
            get { return _Value; }
            set { this.RaiseAndSetIfChanged(ref _Value, value); }
        }


        protected ObservableAsPropertyHelper<IntervalValueViewModel> _ValueType;
        public IntervalValueViewModel ValueType
        {
            get { return _ValueType.Value; }
        }

        public IList<IntervalValueViewModel> ValueTypes { get; protected set; }


    }
}
