using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growthstories.Domain.Messaging;

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


        public IReactiveCommand ItemTappedCommand { get; set; }


        private IDisposable loadSubscription = Disposable.Empty;
        void LoadFriends()
        {
            this.loadSubscription = App.CurrentGardens()
                       .Concat(App.FutureGardens())
                       .Where(x => x.User.Id != App.User.Id)
                       .DistinctUntilChanged()
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(x =>
                       {
                           _Friends.Add(x);

                           this.ListenTo<AggregateDeleted>(x.UserId)
                             .Subscribe(y =>
                             {
                                 _Friends.Remove(x);
                             });
                       });
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
            this.UnFollowCommand
              .RegisterAsyncTask((_) => App.HandleCommand(new DeleteAggregate(this.SelectedFriend.UserId)))
              .Publish()
              .Connect();

        }

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
                        Command = App.Router.NavigateCommandFor<SearchUsersViewModel>()
                    }            
                });
            }
        }

        ReactiveList<IButtonViewModel> _GardenButtons;
        private ReactiveList<IButtonViewModel> GardenButtons
        {
            get
            {
                return _GardenButtons ?? (_GardenButtons = new ReactiveList<IButtonViewModel>() {
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
        }

    }

}
