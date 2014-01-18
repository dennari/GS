
using System;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ReactiveUI;

namespace Growthstories.UI.ViewModel
{


    [DataContract]
    public class MainViewModel : MultipageViewModel, IMainViewModel
    {


        private async Task LoadAsync()
        {

            // it's important to get these through the resolver for ui-reset support

            GardenVM = await Task.Run(() => App.Resolver.GetService<IGardenViewModel>());
            this._Pages.Add(this.GardenVM);
            this.SelectedPage = this.GardenVM;

            NotificationsVM = await Task.Run(() => App.Resolver.GetService<INotificationsViewModel>());
            this._Pages.Add(this.NotificationsVM);

            FriendsVM = await Task.Run(() => App.Resolver.GetService<FriendsViewModel>());
            this._Pages.Add(this.FriendsVM);
        }

        private void Load()
        {
            this.GardenVM = App.Resolver.GetService<IGardenViewModel>();
            this._Pages.Add(this.GardenVM);
            this.SelectedPage = this.GardenVM;

            this.NotificationsVM = App.Resolver.GetService<INotificationsViewModel>();
            this._Pages.Add(this.NotificationsVM);

            this.FriendsVM = App.Resolver.GetService<FriendsViewModel>();
            this._Pages.Add(this.FriendsVM);
        }


        public MainViewModel(IGSAppViewModel app)
            : base(app)
        {
            LoadAsync();
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

