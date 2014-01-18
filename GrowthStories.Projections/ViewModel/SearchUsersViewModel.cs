using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
//using EventStore.Logging;
using EventStore.Logging;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{


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

        private IPopupViewModel _NoConnectionAlert;
        public IPopupViewModel NoConnectionAlert
        {
            get
            {
                if (_NoConnectionAlert == null)
                {


                    _NoConnectionAlert = new PopupViewModel()
                    {
                        Caption = "No data connection available",
                        Message = "Following users requires a data connection. Please enable a data connection and try again.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK"
                    };
                    _NoConnectionAlert.DismissedObservable.Take(1).Select(_ => new object()).Subscribe(App.Router.NavigateBack.Execute);

                }
                return _NoConnectionAlert;

            }
        }


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

        private bool _ValidSearch;
        public bool ValidSearch
        {
            get
            {
                return _ValidSearch;
            }

            private set
            {
                this.RaiseAndSetIfChanged(ref _ValidSearch, value);
            }
        }


        private string _Search;
        public string Search
        {
            get
            {
                return _Search;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Search, value);
                ValidSearch = !string.IsNullOrWhiteSpace(value) && value.Length >= 2;
                SearchFinished = false;
            }
        }

        private bool _SearchFinished;
        public bool SearchFinished
        {
            get
            {
                return _SearchFinished;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SearchFinished, value);
            }
        }

        private bool _NotReachable;
        public bool NotReachable
        {
            get
            {
                return _NotReachable;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _NotReachable, value);
            }

        }

        public SearchUsersViewModel(ITransportEvents transporter, IGSAppViewModel app)
            : base(app)
        {

            SearchFinished = false;
            Transporter = transporter;
            _List = new ReactiveList<RemoteUser>();
            SearchCommand = new ReactiveCommand();
            ProgressIndicatorIsVisible = false;

            var input = SearchCommand
                .OfType<string>()
                .Do(x => this.Search = x)
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
                SearchFinished = true;

                NotReachable = x.StatusCode != GSStatusCode.OK;

                _List.Clear();
                if (x.Users != null && x.Users.Count > 0)
                {
                    var filtered = x.Users.Where(y =>
                        !App.SyncStreams.ContainsKey(y.AggregateId) &&
                        y.Garden != null && y.Garden.Plants != null && y.Garden.Plants.Count > 0
                        ).ToArray();

                    if (filtered.Length > 0)
                    {
                        Logger.Info("Listed {0} users", filtered.Length);
                        _List.AddRange(filtered);

                    }
                }
            });

            this.SearchResults = results;

            UserSelectedCommand = new ReactiveCommand();
            //UserSelectedCommand.Re

            UserSelectedCommand.Subscribe(_ => App.ShowPopup.Execute(App.SyncPopup));
            this.SyncResults = UserSelectedCommand
                .RegisterAsyncTask(async (xx) =>
                {
                    var x = xx as RemoteUser;
                    if (x == null)
                        return null;

                    Logger.Info("Before BecomeFollower");

                    await App.HandleCommand(new BecomeFollower(App.User.Id, x.AggregateId));

                    // now we get the user stream AND info on the plants

                    Logger.Info("Before SyncAll");

                    await App.SyncAll();
                    // now we get the plants too
                    var syncResult = await App.SyncAll();

                    Logger.Info("Before ShowPopup");

                    App.ShowPopup.Execute(null);


                    Logger.Info("Before NavigationStack");

                    if (App.Router.NavigationStack.Count > 0)
                        App.Router.NavigateBack.Execute(null);
                    return syncResult;

                });

            this.SyncResults.Publish().Connect();

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
            get { return false; }
        }



        public IObservable<Tuple<AllSyncResult, GSStatusCode?>> SyncResults { get; private set; }
    }


}
