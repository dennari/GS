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

            input.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => ProgressIndicatorIsVisible = true);

            results.Subscribe(x =>
            {
                ProgressIndicatorIsVisible = false;
                SearchFinished = true;

                NotReachable = x.StatusCode != GSStatusCode.OK;

                _List.Clear();
                if (x.Users != null && x.Users.Count > 0)
                {

                    var followed = App.GetCurrentPYFs();

                    var filtered = x.Users.Where(y =>
                        !followed.Contains(y.AggregateId) 
                        && y.Garden != null 
                        && y.Garden.Plants != null 
                        && y.Garden.Plants.Count > 0
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

            PopupViewModel pp = new ProgressPopupViewModel()
            {
                Caption = "Following user",
                IsLeftButtonEnabled = false
            };


            pp.DismissedObservable.Subscribe(x =>
            {
                this.Log().Info("followed popup dismissed: " + x);
                var pr = (PopupResult)x;

                FollowCanceled = true;

                if (CleanDismiss)
                {
                    if (App.Router.NavigationStack.Count > 0)
                        App.Router.NavigateBack.Execute(null);
                }
            });


            UserSelectedCommand.Subscribe(_ => App.ShowPopup.Execute(pp));
       

            this.SyncResults = UserSelectedCommand
                .RegisterAsyncTask(async (xx) =>
                {
                    CleanDismiss = false;
                    FollowCanceled = false;

                    var x = xx as RemoteUser;
                    if (x == null)
                        return null;

                    Logger.Info("Before BecomeFollower");

                    // do not add the same followed user twice
                    if (!App.GetCurrentPYFs().Contains(x.AggregateId))
                    {
                        await App.HandleCommand(new BecomeFollower(App.User.Id, x.AggregateId));
                    }
            
                    Logger.Info("Before SyncAll");

                    // (1) we get the user stream AND info on the plants
                    // (2) now we get the plants too
                    // (3) as we have the push filtering problem,
                    //     it is good to do one extra sync just to be sure
                    var syncResult = await App.SyncAll();

                    AllSyncResult? syncRes = null;
                    for (int i = 0; i < 3; i++)
                    {
                        syncRes = (await App.SyncAll()).Item1;
                        if (syncRes == AllSyncResult.Error)
                        {
                            // stop on error, so we don't need to wait for 
                            // request timeout * 3 until we get error message
                            break;
                        }
                    }

                    CleanDismiss = true;
                    App.ShowPopup.Execute(null);

                    Logger.Info("follow canceled is " + FollowCanceled);

                    if (!FollowCanceled)
                    {
                        if (syncRes == Sync.AllSyncResult.Error)
                        {
                            Logger.Info("follow was not canceled and received sync error");
                            PopupViewModel pvm = new PopupViewModel()
                            {
                                Caption = "Failed to load data",
                                Message = "Could not load (all) information on followed user. Please try again later.",
                                IsLeftButtonEnabled = true,
                                LeftButtonContent = "OK",
                            };
                            App.ShowPopup.Execute(pvm);
                        }
                    }
                    
                    return syncResult;
                });

            this.SyncResults.Publish().Connect();

            //this.SyncResults.Subscribe(x =>
            //{
            //    if (x.Item1 == Sync.AllSyncResult.Error)
            //    {
            //        PopupViewModel pvm = new PopupViewModel()
            //        {
            //            Caption = "Failed to load data",
            //            Message = "Could not load (all) information on followed user. Please try again later.",
            //            IsLeftButtonEnabled = true,
            //            LeftButtonContent = "OK"
            //        };
            //        App.ShowPopup.Execute(pvm);
            //    }
            //});
        }

        private bool CleanDismiss;
        private bool FollowCanceled;

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
