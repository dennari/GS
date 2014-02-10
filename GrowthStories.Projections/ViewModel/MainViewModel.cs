
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
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
            set
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


        private IGardenViewModel _GardenVM;
        public IGardenViewModel GardenVM
        {
            get
            {
                if (_GardenVM == null)
                    InitGardenVM();
                return _GardenVM;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _GardenVM, value);
            }
        }

        private void InitGardenVM()
        {
            this.GardenVM = MyGardenF();
            CheckIfAllLoaded();
        }

        private void CheckIfAllLoaded()
        {
            if (_GardenVM != null && _NotificationsVM != null && _FriendsVM != null)
                AllLoaded = true;
        }

        private INotificationsViewModel _NotificationsVM;
        public INotificationsViewModel NotificationsVM
        {
            get
            {
                if (_NotificationsVM == null)
                    InitNotificationsVM();
                return _NotificationsVM;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _NotificationsVM, value);

            }
        }

        private void InitNotificationsVM()
        {
            this.NotificationsVM = NotificationsF();
            CheckIfAllLoaded();
        }

        private FriendsViewModel _FriendsVM;
        public FriendsViewModel FriendsVM
        {
            get
            {
                if (_FriendsVM == null)
                    InitFriendsVM();
                return _FriendsVM;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _FriendsVM, value);

            }
        }

        private void InitFriendsVM()
        {
            this.FriendsVM = FriendsF();
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

        public void Dispose()
        {
            if (this.FriendsVM != null)
                this.FriendsVM.Dispose();
            if (this.GardenVM != null)
                this.GardenVM.Dispose();
        }
    }


    public class TestingViewModel : GSViewModelBase
    {
        public TestingViewModel(IGSAppViewModel app)
            : base(app)
        {
            this.CreateLocalDataCommand = new ReactiveCommand(Observable.Return(true), false);
            CreateLocalDataCommand.IsExecuting.ToProperty(this, x => x.CreateLocalDataCommandIsExecuting, out _CreateLocalDataCommandIsExecuting, true);

            this.CreateRemoteDataCommand = new ReactiveCommand();
            this.PushRemoteUserCommand = new ReactiveCommand();
            this.ClearDBCommand = new ReactiveCommand();
            this.SyncCommand = new ReactiveCommand();
            this.PushCommand = new ReactiveCommand();
            this.ResetCommand = new ReactiveCommand();
            this.RegisterCommand = new ReactiveCommand();

            this.RegisterCommand.Subscribe(_ => this.Navigate(new SignInRegisterViewModel(App)));
        }

        protected ObservableAsPropertyHelper<bool> _CreateLocalDataCommandIsExecuting;
        public bool CreateLocalDataCommandIsExecuting
        {
            get { return _CreateLocalDataCommandIsExecuting.Value; }
        }

        public ReactiveCommand CreateLocalDataCommand { get; protected set; }
        public ReactiveCommand CreateRemoteDataCommand { get; protected set; }
        public ReactiveCommand PushRemoteUserCommand { get; protected set; }
        public ReactiveCommand ClearDBCommand { get; protected set; }
        public ReactiveCommand SyncCommand { get; protected set; }
        public ReactiveCommand PushCommand { get; protected set; }
        public ReactiveCommand RegisterCommand { get; protected set; }
        public ReactiveCommand ResetCommand { get; protected set; }



    }


}

