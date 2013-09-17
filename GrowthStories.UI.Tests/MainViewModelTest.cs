using Growthstories.Domain.Messaging;
using Growthstories.Domain;
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

            public TestMultiPageViewModel() : base(null) { }
            public override string UrlPathSegment
            {
                get { throw new NotImplementedException(); }
            }

        }

        public class TestViewModel : RoutableViewModel, IHasAppBarButtons, IHasMenuItems, IControlsAppBar
        {
            public TestViewModel() : base(null) { }
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

            public ReactiveList<MenuItemViewModel> _AppBarMenuItems;
            public ReactiveList<MenuItemViewModel> AppBarMenuItems
            {
                get
                {
                    return _AppBarMenuItems;
                }
                set
                {
                    this.RaiseAndSetIfChanged(ref _AppBarMenuItems, value);
                }
            }

            public ApplicationBarMode AppBarMode { get; set; }

            public bool _AppBarVisibility;
            public bool AppBarIsVisible { get { return _AppBarVisibility; } set { this.RaiseAndSetIfChanged(ref _AppBarVisibility, value); } }

        }


        [Test]
        public void MultiPageViewModelTest()
        {
            var multi = new TestMultiPageViewModel();
            var vm = new TestViewModel()
            {
                AppBarMode = ApplicationBarMode.DEFAULT,
                AppBarIsVisible = false,
                AppBarButtons = new ReactiveList<ButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButton!"
                    }
                },
                AppBarMenuItems = new ReactiveList<MenuItemViewModel>()
                {
                    new MenuItemViewModel(null) {
                        Text = "TestMenuItem"
                    }
                }
            };

            multi.Pages.Add(vm);
            multi.PageChangedCommand.Execute(0);

            Assert.AreSame(vm.AppBarButtons, multi.AppBarButtons);
            Assert.AreSame(vm.AppBarMenuItems, multi.AppBarMenuItems);

            Assert.AreEqual(vm.AppBarMode, multi.AppBarMode);
            Assert.AreEqual(vm.AppBarIsVisible, multi.AppBarIsVisible);


            var vm2 = new TestViewModel()
            {
                AppBarMode = ApplicationBarMode.MINIMIZED,
                AppBarIsVisible = true,
                AppBarButtons = new ReactiveList<ButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButtonN!"
                    }
                },
                AppBarMenuItems = new ReactiveList<MenuItemViewModel>()
                {
                    new MenuItemViewModel(null) {
                        Text = "TestMenuItemM"
                    }
                }
            };

            multi.Pages.Add(vm2);
            multi.PageChangedCommand.Execute(vm2);

            Assert.AreSame(vm2.AppBarButtons, multi.AppBarButtons);
            Assert.AreSame(vm2.AppBarMenuItems, multi.AppBarMenuItems);
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
        public void TestMainViewModel()
        {
            var plant = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id);
            Assert.AreSame(this.Bus, this.App.Bus);
            Bus.SendCommand(plant);
            Bus.SendCommand(new AddPlant(Ctx.GardenId, plant.EntityId, plant.Name));
            Bus.SendCommand(new MarkPlantPublic(plant.EntityId));

            var plant2 = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id);
            Bus.SendCommand(plant2);
            Bus.SendCommand(new AddPlant(Ctx.GardenId, plant2.EntityId, plant2.Name));

            var mvm = App.Resolver.GetService<IMainViewModel>();

            Assert.AreEqual(mvm.GardenVM.Id, Ctx.GardenId);
            Assert.AreEqual(mvm.GardenVM.Plants[0].Id, plant.EntityId);
            Assert.AreEqual(mvm.GardenVM.Plants[1].Id, plant2.EntityId);

        }


    }
}
