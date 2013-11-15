using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using Growthstories.Sync;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading;
using Growthstories.Core;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace Growthstories.DomainTests
{


    public class MainViewModelTest : ViewModelTestBase
    {

        public class TestMultiPageViewModel : MultipageViewModel
        {
            public void AddPage(IGSViewModel page)
            {
                this._Pages.Add(page);
            }
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

            public ReactiveList<IButtonViewModel> _AppBarButtons;
            public IReadOnlyReactiveList<IButtonViewModel> AppBarButtons
            {
                get
                {
                    return _AppBarButtons;
                }
                set
                {
                    this.RaiseAndSetIfChanged(ref _AppBarButtons, (ReactiveList<IButtonViewModel>)value);
                }
            }

            public ReactiveList<IMenuItemViewModel> _AppBarMenuItems;
            public IReadOnlyReactiveList<IMenuItemViewModel> AppBarMenuItems
            {
                get
                {
                    return _AppBarMenuItems;
                }
                set
                {
                    this.RaiseAndSetIfChanged(ref _AppBarMenuItems, (ReactiveList<IMenuItemViewModel>)value);
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
                AppBarButtons = new ReactiveList<IButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButton!"
                    }
                },
                AppBarMenuItems = new ReactiveList<IMenuItemViewModel>()
                {
                    new MenuItemViewModel(null) {
                        Text = "TestMenuItem"
                    }
                }
            };

            multi.AddPage(vm);
            multi.PageChangedCommand.Execute(0);

            Assert.AreSame(vm.AppBarButtons, multi.AppBarButtons);
            Assert.AreSame(vm.AppBarMenuItems, multi.AppBarMenuItems);

            Assert.AreEqual(vm.AppBarMode, multi.AppBarMode);
            Assert.AreEqual(vm.AppBarIsVisible, multi.AppBarIsVisible);


            var vm2 = new TestViewModel()
            {
                AppBarMode = ApplicationBarMode.MINIMIZED,
                AppBarIsVisible = true,
                AppBarButtons = new ReactiveList<IButtonViewModel>()
                {
                    new ButtonViewModel(null) {
                        Text = "TestButtonN!"
                    }
                },
                AppBarMenuItems = new ReactiveList<IMenuItemViewModel>()
                {
                    new MenuItemViewModel(null) {
                        Text = "TestMenuItemM"
                    }
                }
            };

            multi.AddPage(vm2);
            multi.PageChangedCommand.Execute(vm2);

            Assert.AreSame(vm2.AppBarButtons, multi.AppBarButtons);
            Assert.AreSame(vm2.AppBarMenuItems, multi.AppBarMenuItems);
            Assert.AreEqual(vm2.AppBarMode, multi.AppBarMode);
            Assert.AreEqual(vm2.AppBarIsVisible, multi.AppBarIsVisible);

            var old = vm2.AppBarButtons;
            vm2.AppBarButtons = new ReactiveList<IButtonViewModel>()
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
            var plant = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.GardenId, Ctx.Id);
            Assert.AreSame(this.Bus, this.App.Bus);
            App.HandleCommand(plant);
            App.HandleCommand(new AddPlant(Ctx.GardenId, plant.AggregateId, Ctx.Id, plant.Name));
            App.HandleCommand(new MarkPlantPublic(plant.AggregateId));

            var plant2 = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.GardenId, Ctx.Id);
            App.HandleCommand(plant2);
            App.HandleCommand(new AddPlant(Ctx.GardenId, plant2.AggregateId, Ctx.Id, plant2.Name));


            var mvm = App.Resolver.GetService<IMainViewModel>();


            var plants = (IList<IPlantViewModel>)mvm.GardenVM.Plants;
            Assert.AreEqual(mvm.GardenVM.Id, Ctx.GardenId);
            Assert.AreEqual(plants[0].Id, plant.AggregateId);
            Assert.AreEqual(plants[1].Id, plant2.AggregateId);

            var plant3 = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.GardenId, Ctx.Id);
            App.HandleCommand(plant3);
            App.HandleCommand(new AddPlant(Ctx.GardenId, plant3.AggregateId, Ctx.Id, plant3.Name));

            Assert.AreEqual(plants[2].Id, plant3.AggregateId);


        }

        [Test]
        public void TestFriendsViewModel()
        {
            var vm = App.Resolver.GetService<FriendsViewModel>();

            var lvm = App.Resolver.GetService<SearchUsersViewModel>();


            var plant1 = new RemotePlant()
                        {
                            AggregateId = Guid.NewGuid(),
                            Name = "dfgdf"
                        };

            var plant2 = new RemotePlant()
            {
                AggregateId = Guid.NewGuid(),
                Name = "dfgdf"
            };

            var garden = new RemoteGarden()
                {
                    EntityId = Guid.NewGuid(),
                    Plants = new List<RemotePlant>()
                    {
                        plant1,
                        plant2
                    }
                };
            var friend = new RemoteUser()
            {
                AggregateId = Guid.NewGuid(),
                Garden = garden
            };


            //CreateUser friend = null;
            //CreateGarden garden = null;
            var createdStreams = new HashSet<Guid>();
            Bus.Listen<IEvent>().OfType<SyncStreamCreated>().Where(x => x.SyncStreamType == PullStreamType.USER).Subscribe(x =>
            {
                if (createdStreams.Contains(x.StreamId))
                    return;
                createdStreams.Add(x.StreamId);
                var ffriend = new CreateUser(x.StreamId, "Bob", "123", "mail@net.com");
                App.HandleCommand(ffriend);
                var ggarden = new CreateGarden(garden.EntityId, ffriend.AggregateId);
                App.HandleCommand(ggarden);
                App.HandleCommand(new AddGarden(ffriend.AggregateId, ggarden.EntityId.Value));


            });

            int num = 0;
            Bus.Listen<IEvent>().OfType<SyncStreamCreated>().Where(x => x.SyncStreamType == PullStreamType.PLANT).Subscribe(x =>
            {

                if (createdStreams.Contains(x.StreamId))
                    return;
                createdStreams.Add(x.StreamId);
                var plant = new CreatePlant(x.StreamId, "Jore", garden.EntityId, friend.AggregateId);

                App.HandleCommand(plant);
                App.HandleCommand(new AddPlant(garden.EntityId, plant.AggregateId, friend.AggregateId, plant.Name));
                App.HandleCommand(new MarkPlantPublic(plant.AggregateId));



                //Assert.AreEqual(vm.Friends[0].Id, garden.EntityId.Value);
                //Assert.AreEqual(vm.Friends[0].Plants[num].Id, plant.AggregateId);
                num++;
                //Assert.AreEqual(vm.Friends[0].Plants[1].Id, plant2.AggregateId);

            });


            lvm.UserSelectedCommand.Execute(friend);



            var a = "a";

            var friends = (IList<IGardenViewModel>)vm.Friends;
            var plants = (IList<IPlantViewModel>)friends[0].Plants;
            Assert.AreEqual(friends[0].Id, garden.EntityId);
            Assert.AreEqual(plants[0].Id, plant1.AggregateId);
            Assert.AreEqual(plants[1].Id, plant2.AggregateId);


        }

    }
}
