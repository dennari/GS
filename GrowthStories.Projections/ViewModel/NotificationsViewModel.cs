using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Growthstories.Domain.Messaging;
using Growthstories.UI.Services;
using ReactiveUI;
using Growthstories.Domain.Entities;


namespace Growthstories.UI.ViewModel
{


    public enum NotificationType
    {

        WATERING_SCHEDULE,
        FERTILIZING_SCHEDULE,
    }


    public sealed class Notification : IComparable<Notification>
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public double Number { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTimeOffset Date { get; set; }


        public string NotificationText
        {
            get
            {

                ScheduleType SType = ScheduleType.WATERING;
                switch (Type)
                {
                    case NotificationType.WATERING_SCHEDULE:
                        SType = ScheduleType.WATERING;
                        break;

                    case NotificationType.FERTILIZING_SCHEDULE:
                        SType = ScheduleType.FERTILIZING;
                        break;
                }

                return PlantScheduler.NotificationText(Interval, Number, SType, Name.ToUpper());
            }
        }


        public long TicksToAction
        {
            get
            {
                return (long)(Interval.Ticks * Number);
            }
        }


        public int CompareTo(Notification o2)
        {
            return (int)(o2.TicksToAction - this.TicksToAction);
        }

    }


    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {

        private ObservableAsPropertyHelper<IGardenViewModel> _Garden;
        private IGardenViewModel Garden
        {
            get
            {
                return _Garden == null ? null : _Garden.Value;
            }
        }


        private void SetupAppBarButtons()
        {
            this._AppBarButtons.Clear();
            var g = Garden as GardenViewModel;

            if (g != null)
            {
                this._AppBarButtons.Add(
                    new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconType = IconType.ADD,
                        Command = g.TryAddPlantCommand
                    }
                );
            }
        }

        private ObservableAsPropertyHelper<IReactiveDerivedList<IPlantViewModel>> _ScheduledPlants;
        private IReactiveDerivedList<IPlantViewModel> ScheduledPlants
        {
            get
            {
                return _ScheduledPlants != null ? _ScheduledPlants.Value : null;
            }
        }


        public bool IsScheduled(IPlantViewModel x)
        {
            return (x.IsFertilizingScheduleEnabled && x.FertilizingScheduler != null) || (x.IsWateringScheduleEnabled && x.WateringScheduler != null);
        }



        public NotificationsViewModel(IObservable<IGardenViewModel> garden, IGSAppViewModel app)
            : base(app)
        {


            garden
                .Where(x => x != null)
                .ToProperty(this, x => x.Garden, out _Garden);

            var sub0 = this.WhenAnyValue(x => x.Garden)
                .Where(x => x != null)
                .Subscribe(_ => SetupAppBarButtons());

            this.WhenAnyValue(x => x.Garden.Plants)
                .Where(x => x != null)
                .Select(x =>
                {
                    return x.CreateDerivedCollection(y => y, IsScheduled);
                })
                .ToProperty(this, x => x.ScheduledPlants, out _ScheduledPlants);

            var plantStream = this.WhenAnyValue(x => x.ScheduledPlants)
                .Where(x => x != null)
                .Select(x =>
                {
                    return x.ItemsAdded.StartWith(x);
                })
                .Switch();


            var sub1 = plantStream.Where(x => x.IsFertilizingScheduleEnabled && x.FertilizingScheduler != null)
               .SelectMany(x =>
                   Observable.Merge(
                       x.WhenAny(y => y.FertilizingScheduler.Missed, y => 1),
                       x.WhenAnyValue(y => y.FertilizingScheduler.LastActionTime).Where(y => y == null).Select(y => 1) // lastActionTime is null if there are no actions of the correct type
                       ).Select(y => x)
                )
               .Subscribe(x => UpdateFertilizingNotification(x));

            var sub2 = plantStream.Where(x =>
            {
                return x.IsWateringScheduleEnabled && x.WateringScheduler != null;
            })
               .SelectMany(x =>
                   Observable.Merge(
                       x.WhenAny(y => y.WateringScheduler.Missed, y => 1),
                       x.WhenAnyValue(y => y.WateringScheduler.LastActionTime).Where(y => y == null).Select(y => 1)
                       ).Select(y => x)
                )
               .Subscribe(x => UpdateWateringNotification(x));

            var sub3 = this.WhenAnyValue(x => x.ScheduledPlants)
                .Where(x => x != null)
                .Select(x => x.ItemsRemoved)
                .Switch()
                .Subscribe(pvm =>
                {
                    TryRemove(pvm.Id, NotificationType.FERTILIZING_SCHEDULE);
                    TryRemove(pvm.Id, NotificationType.WATERING_SCHEDULE);
                });

            var sub4 = this.WhenAnyValue(x => x.ScheduledPlants)
                .Where(x => x != null && x.Count == 0)
                .Subscribe(_ =>
                {
                    this.Notifications.RemoveAll(this.Notifications.ToArray());
                });

            subs.Add(sub0);
            subs.Add(sub1);
            subs.Add(sub2);
            subs.Add(sub3);
            subs.Add(sub4);


        }


        private void UpdateWateringNotification(IPlantViewModel pvm)
        {
            if (!pvm.IsWateringScheduleEnabled
                || pvm.WateringScheduler == null
                || pvm.WateringScheduler.LastActionTime == null
                )
            {
                TryRemove(pvm.Id, NotificationType.WATERING_SCHEDULE);
            }
            else
            {
                var wn = new Notification()
                {
                    Name = pvm.Name,
                    Id = pvm.Id,
                    Type = NotificationType.WATERING_SCHEDULE,
                    Number = pvm.WateringScheduler.Missed,
                    Interval = pvm.WateringScheduler.Interval.Value
                };
                UpdateList(wn);
            }
        }


        private void UpdateFertilizingNotification(IPlantViewModel pvm)
        {
            if (!pvm.IsFertilizingScheduleEnabled
                || pvm.FertilizingScheduler == null
                || pvm.FertilizingScheduler.LastActionTime == null)
            {
                TryRemove(pvm.Id, NotificationType.FERTILIZING_SCHEDULE);

            }
            else
            {
                var fn = new Notification()
                {
                    Name = pvm.Name,
                    Id = pvm.Id,
                    Type = NotificationType.FERTILIZING_SCHEDULE,
                    Number = pvm.FertilizingScheduler.Missed,
                    Interval = pvm.FertilizingScheduler.Interval.Value
                };
                UpdateList(fn);
            }
        }


        private void TryRemove(Guid id, NotificationType type)
        {
            var found = this.Notifications.FirstOrDefault(y => y.Id == id && y.Type == type);
            if (found != null)
                this.Notifications.Remove(found);
        }


        private static int CompareNotifications(Notification x, Notification y)
        {
            if (x.TicksToAction > y.TicksToAction)
            {
                return -1;
            }
            else if (x.TicksToAction == y.TicksToAction)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }



        private void UpdateList(Notification notification)
        {
            TryRemove(notification.Id, notification.Type);
            Notifications.Add(notification);

            Notifications.Sort(CompareNotifications);


        }


        private Dictionary<Tuple<Guid, NotificationType>, int?> NotificationsForPlant = new Dictionary<Tuple<Guid, NotificationType>, int?>();

        private ReactiveList<Notification> _Notifications = new ReactiveList<Notification>();
        public ReactiveList<Notification> Notifications
        {
            get
            {
                return _Notifications;
            }

        }

        protected ReactiveList<IButtonViewModel> _AppBarButtons = new ReactiveList<IButtonViewModel>();
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get { return _AppBarButtons; }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }


        private bool _AppBarIsVisible = true;
        public bool AppBarIsVisible
        {
            get
            {
                return _AppBarIsVisible;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _AppBarIsVisible, value);
            }
        }


    }
}
