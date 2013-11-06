﻿using ReactiveUI;
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
        public IGardenViewModel SelectedItem { get { return _SelectedItem; } set { this.RaiseAndSetIfChanged(ref _SelectedItem, value); } }

        public IReactiveCommand FriendTapped { get; protected set; }

        protected ReactiveList<IGardenViewModel> _Friends;
        public IReadOnlyReactiveList<IGardenViewModel> Friends
        {
            get
            {
                if (_Friends == null)
                {
                    _Friends = new ReactiveList<IGardenViewModel>();
                    App.CurrentGardens()
                        .Concat(App.FutureGardens())
                        .Where(x => x.UserState.Id != App.Context.CurrentUser.Id)
                        .DistinctUntilChanged()
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x =>
                        {
                            _Friends.Add(x);
                        });

                }
                return _Friends;
            }
        }



        public FriendsViewModel(IGSAppViewModel app)
            : base(app)
        {


            this.FriendTapped = new ReactiveCommand();
            this.FriendTapped.OfType<IGardenViewModel>().Subscribe(x =>
            {
                this.SelectedItem = x;
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
                        IconUri = App.IconUri[IconType.ADD],
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
