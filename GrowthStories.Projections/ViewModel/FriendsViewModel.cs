using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Growthstories.Domain.Messaging;
using ReactiveUI;
using Growthstories.Domain.Entities;
using System.Collections.Generic;

namespace Growthstories.UI.ViewModel
{

    public class FriendsViewModel : MultipageViewModel, IFriendsViewModel
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
                    _FilteredFriends = Friends.ToObservable()
                        .Where(x => !x.Username.Equals(AuthUser.UnregUsername)).CreateCollection();
                }
                return _FilteredFriends;
            }

        }

        public IReactiveCommand ItemTappedCommand { get; set; }

       

        private void LoadFollowedUser(Guid user)
        {
            App.CurrentGardens()
            .Where(x => x.User.Id == user)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                _Friends.Add(x);
            });
        }


        private void RemoveFollowedUser(Guid user)
        {
            // we need to first collect the item(s) to be
            // removed and then actually remove them, as we 
            // cannot modify a collection while traversing it
            //
            // we do this the old fashion way to be sure
            //
            var toBeRemoved = new List<IGardenViewModel>();
            foreach (var f in Friends)
            {
                if (f.UserId == user)
                {
                    toBeRemoved.Add(f);
                }
            }
            foreach (var f in toBeRemoved)
            {
                _Friends.Remove(f);
            }
        }


        private IDisposable loadSubscription = Disposable.Empty;
        void LoadFriends()
        {

            App.GetCurrentFollowers(App.User.Id)
                .ToObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
            {
                var guid = (Guid)x;
                LoadFollowedUser(x);
            });

            this.ListenTo<BecameFollower>(App.User.Id)
            .Subscribe(x =>
            {
                LoadFollowedUser(x.Target);
            });

            this.ListenTo<UnFollowed>(App.User.Id)
            .Subscribe(x =>
            {
                RemoveFollowedUser(x.Target);
            });



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



        protected ReactiveList<IGardenViewModel> _Friends;
        public IReadOnlyReactiveList<IGardenViewModel> Friends
        {
            get
            {
                if (_Friends == null)
                {
                    _Friends = new ReactiveList<IGardenViewModel>();
                    App.WhenAnyValue(x => x.User).Subscribe(x =>
                    {

                        if (x == null)
                        {
                            this.loadSubscription.Dispose();
                            if (_Friends != null)
                            {
                                _Friends.Clear();
                            }
                        }
                        else
                        {
                            this.LoadFriends();
                        }

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
            this.ItemTappedCommand = new ReactiveCommand(App.Router.CurrentViewModel.Select(x => x != this));
            this.ItemTappedCommand.Subscribe(_ => this.Navigate(this));

            this.App.Router.CurrentViewModel.Subscribe(x =>
            {
                if (x == this)
                {
                    this.AppBarButtons = this.GardenButtons;

                }
                else
                {
                    this.AppBarButtons = this.MainViewButtons;

                }
            });

            this.UnFollowCommand = new ReactiveCommand();

            this.TrySearchUsersCommand.Subscribe(_ =>
            {
                if (Friends.Count >= 7)
                {
                    var pvm = new PopupViewModel()
                    {
                        Caption = "Can't follow more users",
                        Message = "You can only follow 7 users at a time. Please remove some followed users before adding new users to follow.",
                        IsLeftButtonEnabled = true,
                        LeftButtonContent = "OK",
                    };

                    App.ShowPopup.Execute(pvm);
                }
                else
                {
                    App.Router.NavigateCommandFor<ISearchUsersViewModel>().Execute(null);
                }
            });

            this.UnFollowCommand.Subscribe(async _ =>
            {
                this.Log().Info("unfollowing user " + this.SelectedFriend.UserId);
                await App.HandleCommand(new UnFollow(App.User.Id, this.SelectedFriend.UserId));
                this.NavigateBack();
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
            protected set
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

    }

}
