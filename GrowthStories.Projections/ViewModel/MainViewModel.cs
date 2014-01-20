
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{


    [DataContract]
    public class MainViewModel : MultipageViewModel, IMainViewModel
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


        public MainViewModel(
            IObservable<IGardenViewModel> myGarden,
            IObservable<INotificationsViewModel> notifications,
            IObservable<FriendsViewModel> friends,
            IGSAppViewModel app)
            : base(app)
        {
            myGarden.ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            {
                this.GardenVM = x;
                this._Pages.Add(x);
                this.SelectedPage = x;
            }).Select(x => new Unit()).Take(1)
            .Concat(notifications.ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            {
                this.NotificationsVM = x;
                this._Pages.Add(x);
            }).Select(x => new Unit()).Take(1))
            .Concat(friends.ObserveOn(RxApp.MainThreadScheduler).Do(x =>
            {
                this.FriendsVM = x;
                this._Pages.Add(x);
            }).Select(x => new Unit()).Take(1))
            .ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => { }, () =>
            {
                AllLoaded = true;
            });
        }


        private IGardenViewModel _GardenVM;
        public IGardenViewModel GardenVM
        {
            get
            {
                return _GardenVM;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _GardenVM, value);
            }
        }

        private INotificationsViewModel _NotificationsVM;
        public INotificationsViewModel NotificationsVM
        {
            get
            {
                return _NotificationsVM;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _NotificationsVM, value);

            }
        }

        private FriendsViewModel _FriendsVM;
        public FriendsViewModel FriendsVM
        {
            get
            {
                return _FriendsVM;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _FriendsVM, value);

            }
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

