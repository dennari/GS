using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{



    public class FriendsViewModel : RoutableViewModel, IFriendsViewModel
    {

        protected IGardenViewModel _SelectedItem;
        public IGardenViewModel SelectedFriend { get { return _SelectedItem; } set { this.RaiseAndSetIfChanged(ref _SelectedItem, value); } }

        public object SelectedItem
        {
            get { return SelectedFriend; }
            set
            {

                var v = value as IGardenViewModel;
                if (v != null)
                    SelectedFriend = v;
            }
        }


        public IReactiveCommand FriendTapped { get; protected set; }


        void LoadFriends()
        {
            App.CurrentGardens()
                       .Concat(App.FutureGardens())
                       .Where(x => x.User.Id != App.User.Id)
                       .DistinctUntilChanged()
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(x =>
                       {
                           _Friends.Add(x);
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
                    if (App.User == null)
                    {
                        App.WhenAny(x => x.User, x => x.GetValue()).Where(x => x != null).Take(1).Subscribe(x => LoadFriends());
                    }
                    else
                    {
                        LoadFriends();
                    }

                }
                return _Friends;
            }
        }



        public FriendsViewModel(IGSAppViewModel app)
            : base(app)
        {


            //this.FriendTapped = new ReactiveCommand();
            //this.FriendTapped.OfType<IGardenViewModel>().Subscribe(x =>
            //{
            //    this.SelectedItem = x;
            //   ;
            //});

            this.WhenAny(x => x.SelectedFriend, x => x.GetValue())
                .Where(x => x != null)
                .Subscribe(_ => App.Router.Navigate.Execute(this));

        }
        protected ReactiveList<IButtonViewModel> _AppBarButtons;
        public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
        {
            get
            {
                return _AppBarButtons ?? (_AppBarButtons = new ReactiveList<IButtonViewModel>() {
                    new ButtonViewModel(null)
                    {
                        Text = "add",
                        IconType = IconType.ADD,
                        Command = App.Router.NavigateCommandFor<SearchUsersViewModel>()
                    }            
                });
            }
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
            get { return true; }
        }
    }

}
