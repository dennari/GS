using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;
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

        /*
        public bool ShouldShow
        {
            get
            {
                return Number > -0.2;
            }
        }
        */

    }

    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {

        private readonly IGardenViewModel Garden;



        public NotificationsViewModel(IGardenViewModel garden, IGSAppViewModel app)
            : base(app)
        {

            AppBarIsVisible = false;
            this._AppBarButtons.Add(
            new ButtonViewModel(null)
            {
                Text = "add",
                IconType = IconType.ADD,
                Command = this.HostScreen.Router.NavigateCommandFor<IAddEditPlantViewModel>()
            });

            this.Garden = garden;

            var currentAndFuturePlants = Garden.Plants.ItemsAdded.StartWith(Garden.Plants);

            IReadOnlyReactiveList<IPlantActionViewModel> latestActions = null;
            currentAndFuturePlants
                .Do(x => latestActions = x.Actions) // this forces the actions to load eagerly, in order to get the notifications in the start
                .SelectMany(x => x.WhenAnyValue(y => y.WateringScheduler, y => y.IsWateringScheduleEnabled, (y1, y2) => Tuple.Create(x, y1, y2)))
                .Where(x => x.Item2 != null)
                .SelectMany(x => x.Item2.WhenAnyValue(y => y.LastActionTime, y => Tuple.Create(x, y)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {

                    var plant = x.Item1.Item1;

                    if (!x.Item1.Item3)
                        TryRemove(plant.Id, NotificationType.WATERING_SCHEDULE);
                    else
                    {
                        var notification = new Notification()
                        {
                            Name = plant.Name,
                            Id = plant.Id,
                            Type = NotificationType.WATERING_SCHEDULE,
                            Number = x.Item1.Item2.Missed,
                            Interval = x.Item1.Item2.Interval.Value
                        };

                        UpdateList(notification);
                    }
                });

            //currentAndFuturePlants
            //    .SelectMany(x => x.WhenAnyValue(y => y.IsWateringScheduleEnabled, y => Tuple.Create(x, y)))
            //    .Where(x => !x.Item2)
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(x =>
            //    {
            //        TryRemove(x.Item1.Id, NotificationType.WATERING_SCHEDULE);

            //    });


            //{
            //    var ws = x.WateringScheduler;
            //    var fs = x.FertilizingScheduler;
            //    x.WhenAnyValue(y => y.WateringScheduler.LastActionTime).Where(y => y != null).ObserveOn(RxApp.MainThreadScheduler).Subscribe(y =>
            //    {

            //        var notification = new Notification()
            //        {
            //            Name = x.Name,
            //            Id = x.Id,
            //            Type = NotificationType.WATERING_SCHEDULE,
            //            Number = ws.Missed
            //        };

            //        UpdateList(notification);

            //    });
            //    x.WhenAnyValue(y => y.FertilizingScheduler.LastActionTime).Where(y => y != null).ObserveOn(RxApp.MainThreadScheduler).Subscribe(y =>
            //    {

            //        var notification = new Notification()
            //        {
            //            Name = x.Name,
            //            Id = x.Id,
            //            Type = NotificationType.FERTILIZING_SCHEDULE,
            //            Number = ws.Missed
            //        };

            //        UpdateList(notification);

            //    });
            //});

        }

        private void TryRemove(Guid id, NotificationType type)
        {
            var found = this.Notifications.FirstOrDefault(y => y.Id == id && y.Type == type);
            if (found != null)
                this.Notifications.Remove(found);
        }

        private void UpdateList(Notification notification)
        {

            TryRemove(notification.Id, notification.Type);
            Notifications.Add(notification);
            //var key = Tuple.Create(notification.Id, NotificationType.WATERING_SCHEDULE);
            //int? index = null;
            //if (NotificationsForPlant.TryGetValue(key, out index))
            //{
            //    Notifications.RemoveAt(index.Value);
            //}
            //NotificationsForPlant[key] = Notifications.Count - 1;

            //Notifications.Sort();           
        }

        /*
        public class Comparer<Notification> : IComparer<Notification>
        {

            int Compare(Notification o1, Notification o2)
            {

                //o1.TicksToAction

                return o1.TicksToAction - o2.TicksToAction;
            }
        }
        */


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
