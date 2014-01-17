﻿using ReactiveUI;
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
    
    }


    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {

        private readonly IGardenViewModel Garden;

      

        public NotificationsViewModel(IGardenViewModel garden, IGSAppViewModel app)
            : base(app)
        {

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
            currentAndFuturePlants.Subscribe(plant =>
            {   
                // this forces the actions to load eagerly, 
                // in order to get the notifications in the start
                latestActions = plant.Actions;
            
                plant
                    .WhenAnyValue(y => y.WateringScheduler)
                    .Where(y => y != null)
                    .Subscribe(z =>
                    {
                        z.WhenAnyValue(ws => ws.Missed).Subscribe(_ => 
                        {
                            UpdateWateringNotification(plant);
                        });
                    });

                plant
                    .WhenAnyValue(y => y.FertilizingScheduler)
                    .Where(y => y != null)
                    .Subscribe(z =>
                    {
                        z.WhenAnyValue(ws => ws.Missed).Subscribe(_ =>
                        {
                            UpdateFertilizingNotification(plant);
                        });
                    });

                plant.WhenAnyValue(y => y.IsWateringScheduleEnabled).Subscribe(_ => UpdateWateringNotification(plant));
                plant.WhenAnyValue(y => y.IsFertilizingScheduleEnabled).Subscribe(_ => UpdateFertilizingNotification(plant));
            });


            Garden.Plants.ItemsRemoved.Subscribe(pvm =>
            {
                TryRemove(pvm.Id, NotificationType.FERTILIZING_SCHEDULE);
                TryRemove(pvm.Id, NotificationType.WATERING_SCHEDULE);
            });

        
            //currentAndFuturePlants
            //    .Do(x => latestActions = x.Actions) // this forces the actions to load eagerly, in order to get the notifications in the start
            //    .SelectMany(x => x.WhenAnyValue(y => y.WateringScheduler, y => y.IsWateringScheduleEnabled, (y1, y2) => Tuple.Create(x, y1, y2)))
            //    .Where(x => x.Item2 != null)
            //    .SelectMany(x => x.Item2.WhenAnyValue(y => y.LastActionTime, y => Tuple.Create(x, y)))
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(x =>
            //    {

            //        var plant = x.Item1.Item1;

            //        if (!x.Item1.Item3)
            //            TryRemove(plant.Id, NotificationType.WATERING_SCHEDULE);
            //        else
            //        {
            //            var notification = new Notification()
            //            {
            //                Name = plant.Name,
            //                Id = plant.Id,
            //                Type = NotificationType.WATERING_SCHEDULE,
            //                Number = x.Item1.Item2.Missed,
            //                Interval = x.Item1.Item2.Interval.Value
            //            };

            //            UpdateList(notification);
            //        }
            //    });
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
