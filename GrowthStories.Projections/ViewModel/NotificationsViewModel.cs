using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Growthstories.Domain.Messaging;
using Growthstories.UI.Services;
using ReactiveUI;
using Growthstories.Domain.Entities;
using Growthstories.Core;

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

        private ObservableAsPropertyHelper<IReactiveDerivedList<IPlantViewModel>> _WateringScheduledPlants;
        private IReactiveDerivedList<IPlantViewModel> WateringScheduledPlants
        {
            get
            {
                return _WateringScheduledPlants != null ? _WateringScheduledPlants.Value : null;
            }
        }


        public bool IsWateringScheduled(IPlantViewModel x)
        {
            return (x.IsWateringScheduleEnabled && x.WateringScheduler != null);
        }

        private ObservableAsPropertyHelper<IReactiveDerivedList<IPlantViewModel>> _FertilizingScheduledPlants;
        private IReactiveDerivedList<IPlantViewModel> FertilizingScheduledPlants
        {
            get
            {
                return _FertilizingScheduledPlants != null ? _FertilizingScheduledPlants.Value : null;
            }
        }


        public bool IsFertilizingScheduled(IPlantViewModel x)
        {
            return (x.IsFertilizingScheduleEnabled && x.FertilizingScheduler != null);
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
                    return x.CreateDerivedCollection(y => y, IsWateringScheduled);
                })
                .ToProperty(this, x => x.WateringScheduledPlants, out _WateringScheduledPlants);

            this.WhenAnyValue(x => x.Garden.Plants)
                .Where(x => x != null)
                .Select(x =>
                {
                    return x.CreateDerivedCollection(y => y, IsFertilizingScheduled);
                })
                .ToProperty(this, x => x.FertilizingScheduledPlants, out _FertilizingScheduledPlants);

            var sub1 = this.WhenAnyValue(x => x.WateringScheduledPlants)
                .Where(x => x != null)
                .Select(x =>
                {
                    return x.ItemsAdded.StartWith(x);
                })
                .Switch()
                .SelectMany(x => x.WhenAny(y => y.WateringScheduler.Missed, y => x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => UpdateWateringNotification(x));


            var sub2 = this.WhenAnyValue(x => x.FertilizingScheduledPlants)
                .Where(x => x != null)
                .Select(x =>
                {
                    return x.ItemsAdded.StartWith(x);
                })
                .Switch()
                .SelectMany(x => x.WhenAny(y => y.FertilizingScheduler.Missed, y => x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => UpdateFertilizingNotification(x));



            var sub3 = this.WhenAnyValue(x => x.WateringScheduledPlants)
                .Where(x => x != null)
                .Select(x => x.ItemsRemoved)
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(pvm =>
                {
                    TryRemove(pvm.Id, NotificationType.WATERING_SCHEDULE);
                });

            var sub31 = this.WhenAnyValue(x => x.FertilizingScheduledPlants)
                .Where(x => x != null)
                .Select(x => x.ItemsRemoved)
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(pvm =>
                {
                    TryRemove(pvm.Id, NotificationType.FERTILIZING_SCHEDULE);
                });

            var sub4 = this.WhenAnyValue(x => x.WateringScheduledPlants) // it's enough to listen to either one
                .Where(x => x != null && x.Count == 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.Log().Info("removing all notifications");
                    this.Notifications.RemoveAll(this.Notifications.ToArray());
                    this.Log().Info("removed all notifications");
                });

            subs.Add(sub0);
            subs.Add(sub1);
            subs.Add(sub2);
            subs.Add(sub3);
            subs.Add(sub31);
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
            //this.Log().Info("tryRemove for {0}", id);
            var found = this.Notifications.FirstOrDefault(y => y.Id == id && y.Type == type);
            if (found != null)
            {
                //this.Log().Info("found is not null, {0}", id); 
                this.Notifications.Remove(found);
                //this.Log().Info("remove ok, {0}", id); 
            }
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


        private AsyncLock UpdateLock = new AsyncLock();
        

        private async void UpdateList(Notification notification)
        {
            using (var res = await UpdateLock.LockAsync())
            {
                //this.Log().Info("updatelist starting for {0}", notification.Id);
                TryRemove(notification.Id, notification.Type);
                Notifications.Add(notification);
                Notifications.Sort(CompareNotifications);
                //this.Log().Info("updatelist ending for {0}", notification.Id);
            };
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
