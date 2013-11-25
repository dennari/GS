using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{

    public enum NotificationType
    {

        WATERING_SCHEDULE,
        FERTILIZING_SCHEDULE,
    }


    public sealed class Notification
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public double Number { get; set; }
        public DateTimeOffset Date { get; set; }


        public string NotificationText
        {
            get
            {
                switch (Type)
                {
                    case NotificationType.WATERING_SCHEDULE:
                        return Name + " should be watered";

                    case NotificationType.FERTILIZING_SCHEDULE:
                        return Name + " should be nourished";
                }
                return "";
            }
        }


        public bool ShouldShow
        {
            get
            {
                return true;
                //return Number > 0.2;
            }
        }

    }

    public class NotificationsViewModel : RoutableViewModel, INotificationsViewModel
    {

        private readonly IGardenViewModel Garden;



        public NotificationsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this._AppBarButtons.Add(
            new ButtonViewModel(null)
            {
                Text = "add",
                IconType = IconType.ADD,
                Command = this.HostScreen.Router.NavigateCommandFor<IAddEditPlantViewModel>()
            });

            this.Garden = app.Resolver.GetService<IGardenViewModel>();

            Garden.Plants.ItemsAdded.StartWith(Garden.Plants)
                .SelectMany(x => x.WhenAnyValue(y => y.WateringScheduler, y => Tuple.Create(x, y)))
                .Where(x => x.Item2 != null)
                .SelectMany(x => x.Item2.WhenAnyValue(y => y.LastActionTime, y => Tuple.Create(x, y)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {

                    var notification = new Notification()
                    {
                        Name = x.Item1.Item1.Name,
                        Id = x.Item1.Item1.Id,
                        Type = NotificationType.WATERING_SCHEDULE,
                        Number = x.Item1.Item2.Missed
                    };

                    UpdateList(notification);

                });






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

        private void UpdateList(Notification notification)
        {
            Notifications.Add(notification);
            var key = Tuple.Create(notification.Id, NotificationType.WATERING_SCHEDULE);
            int? index = null;
            if (NotificationsForPlant.TryGetValue(key, out index))
            {
                Notifications.RemoveAt(index.Value);
            }
            NotificationsForPlant[key] = Notifications.Count - 1;
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
