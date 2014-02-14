using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Growthstories.Domain.Messaging;
using ReactiveUI;
using Growthstories.Domain.Entities;
using System.Collections.Generic;

namespace Growthstories.UI.ViewModel
{

    public sealed class FriendsViewModel : MultipageViewModel, IFriendsViewModel
    {


        private IGardenViewModel _SelectedFriend;
        public IGardenViewModel SelectedFriend
        {
            get
            {
                return _SelectedFriend;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedFriend, value);
            }
        }

        private IReactiveDerivedList<IGardenViewModel> _FilteredFriends;
        public IReactiveDerivedList<IGardenViewModel> FilteredFriends
        {
            get
            {
                if (_FilteredFriends == null)
                {
                    // we need to compare against UnregUsername because 
                    // User.IsRegistered does not probably work
                    _FilteredFriends = Friends.CreateDerivedCollection(x => x, x => !x.Username.Equals(AuthUser.UnregUsername));
                }
                return _FilteredFriends;
            }
        }

        public IReactiveCommand ItemTappedCommand { get; set; }


        private void LoadFollowedUser(Guid user)
        {

        }



        //private IDisposable loadSubscription = Disposable.Empty;
        //private IDisposable unfollowedSubscription = Disposable.Empty;
        
        void LoadFriends()
        {

            this.Log().Info("loading friends for " + App.User.Id);


            IObservable<IGardenViewModel> obs = App.CurrentPYFs();
            if (obs == null)
                obs = App.FuturePYFs();
            else
                obs = obs.Concat(App.FuturePYFs());

            subs.Add(obs.ObserveOn(RxApp.MainThreadScheduler)
               .Subscribe(x =>
               {
                   _Friends.Add(x);
               }));

            subs.Add(this.ListenTo<UnFollowed>(App.User.Id)
            .Subscribe(x =>
            {
                IGardenViewModel friend = Friends.FirstOrDefault(y => y.UserId == x.Target);
                if (friend != null)
                    _Friends.Remove(friend);
                //this._Friends.RemoveAt()
            }));

            //// currentgardens, really? -- JOJ
            //this.loadSubscription = App.CurrentGardens()
            //    .Concat(App.FutureGardens())
            //    .Where(x => x.User.Id != App.User.Id)
            //    .DistinctUntilChanged()
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Subscribe(x =>
            //    {
            //        _Friends.Add(x);
            //        this.ListenTo<UnFollowed>(x.UserId).Subscribe(

            //        this.ListenTo<AggregateDeleted>(x.UserId)
            //            .Subscribe(y =>
            //            {
            //                _Friends.Remove(x);
            //            });
            //    });
        }


        private bool FriendsLoaded = false;
        private ReactiveList<IGardenViewModel> _Friends = new ReactiveList<IGardenViewModel>();
        public IReadOnlyReactiveList<IGardenViewModel> Friends
        {
            get
            {
                if (!FriendsLoaded)
                {
                    FriendsLoaded = true;
                    App
                        .WhenAnyValue(x => x.User)
                        .Where(x => x != null)
                        .Take(1)
                        .Subscribe(x =>
                    {

                        this.LoadFriends();

                    });
                }
                return _Friends;
            }
        }

        public IReactiveCommand UnFollowCommand { get; private set; }


        public FriendsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this.WhenAny(x => x.SelectedPage, x => x.GetValue())
                .Where(x => x != null)
                .OfType<IGardenViewModel>()
                .Subscribe(x => this.SelectedFriend = x);

            this.TrySearchUsersCommand = new ReactiveCommand();

            var isNotThisObs = App.WhenAnyObservable(x => x.Router.CurrentViewModel).Select(x => x != this);

            this.ItemTappedCommand = new ReactiveCommand(isNotThisObs);
            this.ItemTappedCommand.Subscribe(_ => this.Navigate(this));

            subs.Add(isNotThisObs.Subscribe(x =>
            {
                if (!x)
                {
                    this.AppBarButtons = this.GardenButtons;

                }
                else
                {
                    this.AppBarButtons = this.MainViewButtons;

                }
            }));

            this.UnFollowCommand = new ReactiveCommand();

            this.TrySearchUsersCommand.Subscribe(_ =>
            {
                if (Friends.Count >= 5)
                {
                    var pvm = new PopupViewModel()
                    {
                        Caption = "Can't follow more users",
                        Message = "You can only follow 5 users at a time. Please remove some followed users before adding new users to follow.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK",
                    };

                    App.ShowPopup.Execute(pvm);
                }
                else
                {
                    App.Router.Navigate.Execute(App.SearchUsersViewModelFactory(this));
                }
            });

            this.UnFollowCommand.Subscribe(async _ =>
            {
                this.Log().Info("unfollowing user " + this.SelectedFriend.UserId);
                await App.HandleCommand(new UnFollow(App.User.Id, this.SelectedFriend.UserId));
                if (App.Router.NavigationStack.Count > 0)
                {
                    this.NavigateBack();
                }
            });
        }


        public IReactiveCommand TrySearchUsersCommand;

        ReactiveList<IButtonViewModel> _MainViewButtons;
        private ReactiveList<IButtonViewModel> MainViewButtons
        {
            get
            {
                return _MainViewButtons ?? (_MainViewButtons = new ReactiveList<IButtonViewModel>() {
                    new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconType = IconType.ADD,
                        Command = TrySearchUsersCommand
                    }            
                });
            }
        }

        ReactiveList<IButtonViewModel> _GardenButtons;
        private ReactiveList<IButtonViewModel> GardenButtons
        {
            get
            {
                return _GardenButtons ?? (_GardenButtons = new ReactiveList<IButtonViewModel>()
                {
                    new ButtonViewModel(null)
                    {
                        Text = "unfollow",
                        IconType = IconType.CANCEL,
                        Command = this.UnFollowCommand
                    } 
                });
            }
        }

        private new IReadOnlyReactiveList<IButtonViewModel> _AppBarButtons;
        public new IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _AppBarButtons, value);
            }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        private new ApplicationBarMode _AppBarMode = ApplicationBarMode.MINIMIZED;
        public new ApplicationBarMode AppBarMode
        {
            get { return _AppBarMode; }
            set { this.RaiseAndSetIfChanged(ref _AppBarMode, value); }
        }

        public new bool AppBarIsVisible
        {
            get { return true; }
            //get { return this.AppBarButtons == this.MainViewButtons; }
        }


        public override void Dispose()
        {
            base.Dispose();

            foreach (var friend in this.Friends)
            {
                friend.Dispose();
            }
            _Friends.Clear();
        }
    }

}
