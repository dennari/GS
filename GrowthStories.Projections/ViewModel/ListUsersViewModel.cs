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
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{

    //public sealed class RemoteUser
    //{
    //    public readonly string Name;


    //    public RemoteUser(string name)
    //    {
    //        this.Name = name;
    //    }
    //}

    public sealed class ListUsersViewModel : RoutableViewModel, IControlsAppBar
    {
        private readonly ITransportEvents Transporter;

        public ReactiveList<RemoteUser> List { get; private set; }
        public ReactiveCommand SearchCommand { get; private set; }
        public ReactiveCommand UserSelectedCommand { get; private set; }
        public bool InProgress { get; private set; }
        public ListUsersViewModel(ITransportEvents transporter, IGSApp app)
            : base(app)
        {

            Transporter = transporter;
            List = new ReactiveList<RemoteUser>();
            SearchCommand = new ReactiveCommand();
            InProgress = false;

            var input = SearchCommand
                .OfType<string>()
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 2)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .DistinctUntilChanged();



            var results = input
                .Select(s =>
                {
                    return transporter.ListUsersAsync(s).ToObservable();
                })
                .Merge()
                .ObserveOn(RxApp.MainThreadScheduler);


            input.Subscribe(_ => InProgress = true);
            results.Subscribe(x =>
            {
                InProgress = false;
                List.Clear();
                if (x.Users != null && x.Users.Count > 0)
                {

                    List.AddRange(x.Users);
                }
            });



            UserSelectedCommand = new ReactiveCommand();
            UserSelectedCommand
                .OfType<RemoteUser>()
                .Subscribe(x =>
                {
                    this.SendCommand(new CreateSyncStream(x.AggregateId, Core.StreamType.USER));

                    if (x.Garden != null && x.Garden.Plants != null)
                    {
                        foreach (var p in x.Garden.Plants)
                            this.SendCommand(new CreateSyncStream(p.AggregateId, Core.StreamType.PLANT, x.AggregateId));

                    }

                    //this.ListenTo<SyncStreamCreated>().Subscribe(_ => Task.Run(async () => await App.Synchronize()));
                    //App.Router.NavigateBack.Execute(null);
                });

        }


        public void CreateLocalUser(RemoteUser r)
        {

        }


        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.DEFAULT; }
        }

        public bool AppBarIsVisible
        {
            get { return false; }
        }
    }


    public sealed class ListUsersViewModelDesign
    {

        public IList<RemoteUser> List { get; private set; }
        public bool InProgress { get; private set; }
        public ListUsersViewModelDesign()
        {
            List = new List<RemoteUser>() 
            {
                new RemoteUser()
                {
                    Username = "Lauri"
                },
                new RemoteUser()
                {
                    Username = "Jonathan"
                }
            };

        }


    }



}
