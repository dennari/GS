using Growthstories.Core;
using Growthstories.Domain.Messaging;
using Growthstories.UI.ViewModel;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Growthstories.UI.Services
{


    public class GardenScheduler : IScheduleService, IHasLogger
    {
        private readonly TimeSpan UpdateInterval;

        private IGardenViewModel garden;
        private readonly IObservable<long> Timer;

        public GardenScheduler(TimeSpan updateInterval)
        {
            this.UpdateInterval = updateInterval;

            this.Timer = Observable.Interval(updateInterval, RxApp.TaskpoolScheduler);

            this.Timer.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => RecalculateSchedules());
        }

        public IDisposable ScheduleGarden(IGardenViewModel x)
        {

            this.garden = x;
            return Disposable.Empty;
        }


        protected void RecalculateSchedules()
        {
            if (garden == null || garden.Plants == null)
                return;
            foreach (var plant in garden.Plants)
            {
                if (plant.WateringScheduler != null)
                {
                    this.GSLog().Info("Recomputing watering schedule for plant {0}", plant.Name);
                    plant.WateringScheduler.ComputeNext();
                }
                if (plant.FertilizingScheduler != null)
                {
                    plant.FertilizingScheduler.ComputeNext();
                }
            }

        }

        public IGSLog Logger { get; set; }

    }



    public class PluralFormatProvider : IFormatProvider, ICustomFormatter
    {

        public object GetFormat(Type formatType)
        {
            return this;
        }


        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            string[] forms = format.Split(';');
            int value = (int)arg;
            int form = value == 1 ? 0 : 1;
            return value.ToString() + " " + forms[form];
        }

    }


    public sealed class PlantScheduler : ReactiveObject
    {
        public readonly IScheduleViewModel Schedule;


        public const double WINDOW = 0.2;


        public static string IntervalText(long ticks, double Missed)
        {

            var t = new TimeSpan((long)(ticks * Math.Abs(Missed)));

            return IntervalText(t);

        }

        public static string IntervalText(TimeSpan t)
        {
            string IntervalText;

            if (t.TotalWeeks() > 0)
            {
                int numWeeks = t.TotalWeeks();
                int numDays = t.DaysAfterWeeks();
                IntervalText = string.Format("{0:D} {1}, {2:D} {3}", numWeeks, "week".ToPlural(numWeeks), numDays, "day".ToPlural(numDays));
            }
            else if (t.Days > 0)
            {

                IntervalText = string.Format("{0:%d} {1}, {2:D} {3}", t, "day".ToPlural(t.Days), t.Hours, "hour".ToPlural(t.Hours));
            }
            else if (t.Hours > 0)
            {
                IntervalText = string.Format("{0:%h} {1}", t, "hour".ToPlural(t.Hours));
            }
            else
            {
                IntervalText = string.Format("{0:%m} {1}", t, "minute".ToPlural(t.Minutes));
            }
            return IntervalText;

        }


        public static string NotificationText(TimeSpan Interval, double Missed, ScheduleType Type, String Name)
        {
            var t = new TimeSpan((long)(Interval.Ticks * Math.Abs(Missed)));

            // if now is time to water
            if ((Missed < WINDOW && Missed > -WINDOW))
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "Time to water " + Name.ToUpper() + "!";

                    case ScheduleType.FERTILIZING:
                        return "Time to nourish " + Name.ToUpper() + "!";
                }
            }

            // if watering is after some time
            if (Missed <= -WINDOW)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return Name.ToUpper() + " should be watered in " + IntervalText(Interval.Ticks, Missed);

                    case ScheduleType.FERTILIZING:
                        return Name.ToUpper() + " should be nourished in " + IntervalText(Interval.Ticks, Missed);
                }
            }

            // if we have missed a single watering
            if (Missed >= WINDOW && Missed < 1)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return Name.ToUpper() + " needs watering";

                    case ScheduleType.FERTILIZING:
                        return Name.ToUpper() + " needs nourishing";
                }
            }

            // missed more than one watering
            return string.Format(
                "Last {0} {1} missed for {2}",
                PlantScheduler.GetMissedCount(Missed),
                Type == ScheduleType.WATERING ? "waterings" : "nourishments",
                Name.ToUpper());
        }


        public static string NotificationText(TimeSpan Interval, double Missed, ScheduleType Type)
        {
            var t = new TimeSpan((long)(Interval.Ticks * Math.Abs(Missed)));

            // if now is time to water
            if ((Missed < WINDOW && Missed > -WINDOW))
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "Time to water!";

                    case ScheduleType.FERTILIZING:
                        return "Time to nourish!";
                }

            }

            // if watering is not late
            if (Missed <= -WINDOW)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "watering in " + IntervalText(Interval.Ticks, Missed);

                    case ScheduleType.FERTILIZING:
                        return "nourishing in " + IntervalText(Interval.Ticks, Missed);
                }
            }

            // if we are late but not very late
            if (Missed >= WINDOW && Missed < 1)
            {
                switch (Type)
                {
                    case ScheduleType.WATERING:
                        return "needs watering";

                    case ScheduleType.FERTILIZING:
                        return "needs nourishing";
                }
            }

            // missed more than one watering
           return string.Format(
                "Last {0} {1} missed",
                PlantScheduler.GetMissedCount(Missed),
                Type == ScheduleType.WATERING ? "waterings" : "nourishments");
        
        }


        public PlantScheduler(IScheduleViewModel vm, bool own)
        {
            this.Schedule = vm;
            this._OwnPlant = own;

            this.WhenAnyValue(x => x.LastActionTime)
                //  .Where(x => x.HasValue)
                  .Subscribe(x => this.ComputeNext());

        }


        private bool _OwnPlant;

        private DateTimeOffset? _LastActionTime;
        public DateTimeOffset? LastActionTime
        {
            get
            {
                return _LastActionTime;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LastActionTime, value);
            }
        }

        public TimeSpan? Interval
        {
            get { return this.Schedule.Interval; }
        }


        /*
         * Calculate time for next scheduled event
         */
        public static DateTimeOffset ComputeNext(DateTimeOffset last, TimeSpan Interval)
        {
            if (Interval == null)
                throw new InvalidOperationException("This schedule is unspecified, so the next occurence cannot be computed.");

            return last + Interval;
        }


        public static double CalculateMissed(DateTimeOffset last, TimeSpan Interval)
        {
            var next = ComputeNext(last, Interval);
            var now = DateTimeOffset.UtcNow;
            var passedSeconds = (long)(now - next).TotalSeconds;

            var num = (double)passedSeconds / Interval.TotalSeconds;

            return num;
        }


        public static int GetMissedCount(double missed)
        {
            if (missed > WINDOW)
            {
                return (int)Math.Floor(missed) + 1;
                //return (int)(missed * 1000 + 1) % 100;
            }
            return 0;
        }


        public DateTimeOffset ComputeNext()
        {
            if (!Schedule.Interval.HasValue)
                return DateTimeOffset.MinValue;
            if (!LastActionTime.HasValue)
                return DateTimeOffset.MinValue;

            var last = LastActionTime.Value;
            var ans = ComputeNext(last, (TimeSpan)Schedule.Interval);

            this.Missed = CalculateMissed(last, (TimeSpan)Schedule.Interval);

            string temp = PlantScheduler.NotificationText(Schedule.Interval.Value, this.Missed, Schedule.Type);
            this.MissedText = char.ToUpper(temp[0]) + temp.Substring(1);

            this.WeekDay = SharedViewHelpers.FormatWeekDay(ans);
            this.Date = ans.ToString("d");
            this.Time = ans.ToString("t");

            return ans;
        }


        // ratio of the time-intervals of how long has passed and how much can pass (the schedule interval)
        // is negative if next scheduled action is in the future
        private double _Missed;
        public double Missed
        {
            get
            {
                return _Missed;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _Missed, value);
                UpdateMissedNotification();
                UpdateMissedLate();
                UpdateNow();
            }
        }


        private void UpdateMissedNotification()
        {
            MissedNotification = GetMissedCount(Missed).ToString();
        }


        private string _MissedNotification;
        public string MissedNotification
        {
            get
            {
                return _MissedNotification;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _MissedNotification, value);
            }
        }


        public void UpdateNow()
        {
            Now = Missed > -WINDOW;
        }

        // Should this plant be watered now
        private bool _Now;
        public bool Now
        {
            get
            {
                return _Now;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Now, value);
            }
        }


        private void UpdateMissedLate()
        {
            MissedLate = Missed > WINDOW;
        }


        private bool _MissedLate;
        public bool MissedLate
        {
            get
            {
                return _MissedLate;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _MissedLate, value);
                this.UpdateMissedLateAndOwn();
            }
        }

        private void UpdateMissedLateAndOwn()
        {
            MissedLateAndOwn = MissedLate && _OwnPlant;
        }


        private bool _MissedLateAndOwn;
        public bool MissedLateAndOwn
        {
            get
            {
                return _MissedLate;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _MissedLateAndOwn, value);
            }

        }

        private string _MissedText;
        public string MissedText
        {
            get
            {
                return _MissedText;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _MissedText, value);
            }
        }

        private IconType _Icon;
        public IconType Icon
        {
            get
            {
                return _Icon;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Icon, value);
            }
        }

        private string _WeekDay;
        public string WeekDay
        {
            get
            {
                return _WeekDay;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _WeekDay, value);
            }
        }

        private string _Date;
        public string Date
        {
            get
            {
                return _Date;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Date, value);
            }
        }

        private string _Time;
        public string Time
        {
            get
            {
                return _Time;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Time, value);
            }
        }
    }

}
