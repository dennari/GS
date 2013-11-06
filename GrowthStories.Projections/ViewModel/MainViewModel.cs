
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;

namespace Growthstories.UI.ViewModel
{





    [DataContract]
    public class MainViewModel : MultipageViewModel, IMainViewModel
    {



        public MainViewModel(IGSAppViewModel app, IGardenViewModel gvm = null)
            : base(app)
        {

            if (gvm != null)
                this.GardenVM = gvm;


            this._Pages.Add(this.GardenVM);
            this._Pages.Add(this.NotificationsVM);
            this._Pages.Add(this.FriendsVM);

            this.CurrentPage = this.GardenVM;

            //app.Gardens
            //    .Where(x => x.UserState.Id == app.Context.CurrentUser.Id)
            //    .Subscribe(x =>
            //    {
            //        this.GardenVM = x;
            //    });

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
                return _NotificationsVM ?? (_NotificationsVM = App.Resolver.GetService<INotificationsViewModel>());
            }
        }

        private FriendsViewModel _FriendsVM;
        public FriendsViewModel FriendsVM
        {
            get
            {
                return _FriendsVM ?? (_FriendsVM = App.Resolver.GetService<FriendsViewModel>());
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
            this.CreateLocalDataCommand = new ReactiveCommand();
            this.CreateRemoteDataCommand = new ReactiveCommand();
            this.ClearDBCommand = new ReactiveCommand();
            this.SyncCommand = new ReactiveCommand();



        }

        public ReactiveCommand CreateLocalDataCommand { get; protected set; }
        public ReactiveCommand CreateRemoteDataCommand { get; protected set; }
        public ReactiveCommand ClearDBCommand { get; protected set; }
        public ReactiveCommand SyncCommand { get; protected set; }



    }


}

