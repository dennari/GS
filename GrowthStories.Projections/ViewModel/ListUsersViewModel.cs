using Growthstories.Domain.Entities;
using Growthstories.Domain;
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
using Growthstories.Core;
using EventStore.Logging;
using CommonDomain;
using System.Collections;

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

    public sealed class SearchUsersViewModel : RoutableViewModel, ISearchUsersViewModel
    {
        private readonly ITransportEvents Transporter;

        public readonly IObservable<IUserListResponse> SearchResults;
        public readonly IObservable<List<CreateSyncStream>> SyncStreams;

        private static ILog Logger = LogFactory.BuildLogger(typeof(SearchUsersViewModel));


        private ReactiveList<RemoteUser> _List;
        public IReadOnlyReactiveList<RemoteUser> List { get { return _List; } }
        public IReactiveCommand SearchCommand { get; private set; }
        public IReactiveCommand UserSelectedCommand { get; private set; }

        private bool _InProgress;
        public bool ProgressIndicatorIsVisible
        {
            get
            {
                return _InProgress;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _InProgress, value);
            }
        }


        public SearchUsersViewModel(ITransportEvents transporter, IGSAppViewModel app)
            : base(app)
        {

            Transporter = transporter;
            _List = new ReactiveList<RemoteUser>();
            SearchCommand = new ReactiveCommand();
            ProgressIndicatorIsVisible = false;

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
                .ObserveOn(RxApp.MainThreadScheduler)
                .Publish()
                .RefCount();


            input.Subscribe(_ => ProgressIndicatorIsVisible = true);

            results.Subscribe(x =>
            {
                ProgressIndicatorIsVisible = false;
                _List.Clear();
                if (x.Users != null && x.Users.Count > 0)
                {

                    _List.AddRange(x.Users);
                }
            });

            this.SearchResults = results;

            UserSelectedCommand = new ReactiveCommand();
            //UserSelectedCommand.Re

            UserSelectedCommand.Subscribe(_ => ProgressIndicatorIsVisible = true);

            UserSelectedCommand
                .OfType<RemoteUser>()
                .Subscribe(x =>
                {
                    var cmds = new StreamSegment(GSAppState.GSAppId);
                    cmds.Add(new CreateSyncStream(x.AggregateId, Core.PullStreamType.USER));

                    if (x.Garden != null && x.Garden.Plants != null)
                    {
                        foreach (var p in x.Garden.Plants)
                            cmds.Add(new CreateSyncStream(p.AggregateId, Core.PullStreamType.PLANT, x.AggregateId));

                    }

                    App.Bus.SendCommands(cmds);

                });


            /*
            this.SyncStreams
                .Subscribe(x =>
                {
                    ProgressIndicatorIsVisible = false;
                    App.Router.NavigateBack.Execute(null);
                });
            */


        }




        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public ApplicationBarMode AppBarMode
        {
            get { return ApplicationBarMode.MINIMIZED; }
        }

        public bool AppBarIsVisible
        {
            get { return false; }
        }


        public bool SystemTrayIsVisible
        {
            get { return true; }
        }
    }






}
