
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{


    public sealed class MainViewModel : MultipageViewModel, IMainViewModel
    {

        private bool _AllLoaded;
        public bool AllLoaded
        {
            get
            {
                return _AllLoaded;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _AllLoaded, value);
            }
        }

        private Func<IGardenViewModel> MyGardenF;
        private Func<INotificationsViewModel> NotificationsF;
        private Func<FriendsViewModel> FriendsF;

        public MainViewModel(
            Func<IGardenViewModel> myGardenF,
            Func<INotificationsViewModel> notificationsF,
            Func<FriendsViewModel> friendsF,
            IGSAppViewModel app)
            : base(app)
        {
            this.MyGardenF = myGardenF;
            this.NotificationsF = notificationsF;
            this.FriendsF = friendsF;

            //this.Log().Info("Constructor start");
            //myGarden.ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            //{
            //    this.GardenVM = x;
            //    this._Pages.Add(x);
            //    this.SelectedPage = x;
            //}).Select(x => new Unit()).Take(1)
            //.Concat(notifications.ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            //{
            //    this.NotificationsVM = x;
            //    this._Pages.Add(x);
            //}).Select(x => new Unit()).Take(1))
            //.Concat(friends.ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            //{
            //    this.FriendsVM = x;
            //    this._Pages.Add(x);
            //}).Select(x => new Unit()).Take(1))
            //.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => { }, () =>
            //{
            //    AllLoaded = true;
            //});
            //this.Log().Info("Constructor end");

        }


        Task LoadingGarden;
        private IGardenViewModel _GardenVM;
        public IGardenViewModel GardenVM
        {
            get
            {
                if (_GardenVM == null && LoadingGarden == null)
                {
                    LoadingGarden = InitGardenVM();
                }
                return _GardenVM;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _GardenVM, value);
            }
        }

        private async Task InitGardenVM()
        {
            this.GardenVM = await Task.Run(() => MyGardenF());
            CheckIfAllLoaded();
        }

        private void CheckIfAllLoaded()
        {
            if (_GardenVM != null && _NotificationsVM != null && _FriendsVM != null)
            {

                AllLoaded = true;
                this._Pages.Add(_GardenVM);
                this._Pages.Add(_NotificationsVM);
                this._Pages.Add(_FriendsVM);
                this._Pages.Add(TestingVM);
                this.SelectedPage = _GardenVM;
            }
        }

        Task LoadingNotifications;
        private INotificationsViewModel _NotificationsVM;
        public INotificationsViewModel NotificationsVM
        {
            get
            {
                if (_NotificationsVM == null && LoadingNotifications == null)
                {
                    LoadingNotifications = InitNotificationsVM();
                }
                return _NotificationsVM;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _NotificationsVM, value);

            }
        }

        private async Task InitNotificationsVM()
        {

            this.NotificationsVM = await Task.Run(() => NotificationsF());

            CheckIfAllLoaded();
        }

        Task LoadingFriends;
        private FriendsViewModel _FriendsVM;
        public FriendsViewModel FriendsVM
        {
            get
            {
                if (_FriendsVM == null && LoadingFriends == null)
                {
                    LoadingFriends = InitFriendsVM();
                }
                return _FriendsVM;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _FriendsVM, value);

            }
        }

        private async Task InitFriendsVM()
        {
            this.FriendsVM = await Task.Run(() => FriendsF());
            CheckIfAllLoaded();
        }

        private TestingViewModel _TestingVM;

        public TestingViewModel TestingVM
        {
            get
            {
                var vm = _TestingVM ?? (_TestingVM = App.Resolver.GetService<TestingViewModel>());

                //vm.AddTestDataCommandAsync.Subscribe(_ => GardenVM = App.GardenFactory(App.Context.CurrentUser.Id));
                //vm.ClearDBCommandAsync.Subscribe(_ => GardenVM = App.GardenFactory(App.Context.CurrentUser.Id));

                return vm;
            }
        }

        public override string UrlPathSegment
        {
            get { throw new NotImplementedException(); }
        }

        public bool SystemTrayIsVisible
        {
            get { return false; }
        }

        public bool ProgressIndicatorIsVisible
        {
            get { return false; }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (this.FriendsVM != null)
                this.FriendsVM.Dispose();
            if (this.GardenVM != null)
                this.GardenVM.Dispose();
        }
    }


}

