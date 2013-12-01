using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.ViewModel
{



    public class FriendsViewModel : MultipageViewModel, IFriendsViewModel
    {

        private IGardenViewModel _SelectedFriend;
        public IGardenViewModel SelectedFriend { get { return _SelectedFriend; } set { this.RaiseAndSetIfChanged(ref _SelectedFriend, value); } }



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



        public FriendsViewModel(IGSAppViewModel app)
            : base(app)
        {

            this.WhenAny(x => x.SelectedPage, x => x.GetValue())
                .Where(x => x != null)
                .OfType<IGardenViewModel>()
                .Subscribe(x => this.SelectedFriend = x);

            this.WhenAnyValue(x => x.SelectedFriend)
                .Where(x => x != null && App.Router.GetCurrentViewModel() != this)
                .Subscribe(_ =>
                {
                    App.Router.Navigate.Execute(this);
                });

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
