using Growthstories.Domain.Messaging;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading;


namespace Growthstories.UI.Tests
{


    public class MainViewModelTest : ViewModelTestBase
    {

        public class TestMultiPageViewModel : MultipageViewModel
        {

            public TestMultiPageViewModel() : base(null, null, null) { }
            public override string UrlPathSegment
            {
                get { throw new NotImplementedException(); }
            }
        }

        public class TestViewModel : RoutableViewModel, IHasAppBarButtons, IControlsAppBar
        {
            public TestViewModel() : base(null, null, null) { }
            public override string UrlPathSegment
            {
                get { throw new NotImplementedException(); }
            }

            public ReactiveList<ButtonViewModel> _AppBarButtons;
            public ReactiveList<ButtonViewModel> AppBarButtons
            {
                get
                {
                    return _AppBarButtons;
                }
                set
                {
                    this.RaiseAndSetIfChanged(ref _AppBarButtons, value);
                }
            }

            public string AppBarMode { get; set; }

            public bool _AppBarVisibility;
            public bool AppBarIsVisible { get { return _AppBarVisibility; } set { this.RaiseAndSetIfChanged(ref _AppBarVisibility, value); } }

        }

        IScreen Screen;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var Resolver = RxApp.MutableResolver;
            Resolver.Register(() => Get<MainViewModel>(), typeof(IMainViewModel));
            Resolver.Register(() => new GardenViewModel(Guid.NewGuid(), null, Get<IUserService>(), Get<IMessageBus>(), this.Screen), typeof(IGardenViewModel));
            Resolver.Register(() => new NotificationsViewModel(Get<IUserService>(), Get<IMessageBus>(), this.Screen), typeof(INotificationsViewModel));
            Resolver.Register(() => new FriendsViewModel(Get<IUserService>(), Get<IMessageBus>(), this.Screen), typeof(IFriendsViewModel));

        }

        [Test]
        public void MultiPageViewModelTest()
        {
            var multi = new TestMultiPageViewModel();
            var vm = new TestViewModel()
            {
                AppBarMode = "BLAAA!",
                AppBarIsVisible = false,
                AppBarButtons = new ReactiveList<ButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButton!"
                    }
                }
            };

            multi.Pages.Add(vm);
            multi.PageChangedCommand.Execute(0);

            Assert.AreSame(vm.AppBarButtons, multi.AppBarButtons);
            Assert.AreEqual(vm.AppBarMode, multi.AppBarMode);
            Assert.AreEqual(vm.AppBarIsVisible, multi.AppBarIsVisible);


            var vm2 = new TestViewModel()
            {
                AppBarMode = "BLAAA!!",
                AppBarIsVisible = true,
                AppBarButtons = new ReactiveList<ButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButtonN!"
                    }
                }
            };

            multi.Pages.Add(vm2);
            multi.PageChangedCommand.Execute(vm2);

            Assert.AreSame(vm2.AppBarButtons, multi.AppBarButtons);
            Assert.AreEqual(vm2.AppBarMode, multi.AppBarMode);
            Assert.AreEqual(vm2.AppBarIsVisible, multi.AppBarIsVisible);

            var old = vm2.AppBarButtons;
            vm2.AppBarButtons = new ReactiveList<ButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButtonNN!"
                    }
                };
            Assert.AreSame(vm2.AppBarButtons, multi.AppBarButtons);
            Assert.AreNotSame(old, multi.AppBarButtons);

            vm2.AppBarIsVisible = false;
            Assert.AreEqual(vm2.AppBarIsVisible, multi.AppBarIsVisible);


        }

        [Test]
        public void AppBarTest()
        {

            var screen = new AppViewModel();
            this.Screen = screen;
            var mvm = new MainViewModel(Get<IUserService>(), MessageBus.Current, screen);
            var events = mvm.Changed.CreateCollection();
            var gvm = mvm.GardenVM;
            var buttons = gvm.AppBarButtons;

            //mvm.PanoramaPageChangedCommand.Execute(0);
            Assert.AreSame(gvm, mvm.CurrentPage);
            //Console.WriteLine(this.toJSON(gvm.AppBarButtons.ToArray()));
            //Console.WriteLine(this.toJSON(mvm.AppBarButtons.ToArray()));
            Assert.AreSame(buttons, mvm.AppBarButtons);
        }


    }
}
