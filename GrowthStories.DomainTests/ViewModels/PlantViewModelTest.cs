using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading;
using Growthstories.Domain.Entities;
using Growthstories.Sync;


namespace Growthstories.DomainTests
{


    public class PlantViewModelTest : ViewModelTestBase
    {

        protected PlantState State;

        protected IPlantViewModel Create(IScheduleViewModel wateringSchedule = null, IScheduleViewModel fertilizingSchedule = null)
        {



            var name = "Sepi";
            var id = Guid.NewGuid();

            var plant = (Plant)Handler.Handle(new CreatePlant(id, name, U.GardenId, U.Id));
            this.State = plant.State;
            var vm = new PlantViewModel(State, wateringSchedule, fertilizingSchedule, App);
            Assert.AreEqual(id, vm.Id);
            Assert.AreEqual(name, vm.Name);
            Assert.AreEqual(id, State.Id);
            Assert.AreEqual(name, State.Name);
            return vm;



        }

        [Test]
        public void TestUIProjection()
        {
            Create();
            var wS = new CreateSchedule(Guid.NewGuid(), 2 * 24 * 3600);
            Handler.Handle(wS);
            var fS = new CreateSchedule(Guid.NewGuid(), 60 * 24 * 3600);
            Handler.Handle(fS);
            Handler.Handle(new SetWateringSchedule(State.Id, wS.AggregateId));
            Handler.Handle(new SetFertilizingSchedule(State.Id, fS.AggregateId));

            Assert.AreEqual(wS.AggregateId, State.WateringScheduleId);
            Assert.AreEqual(fS.AggregateId, State.FertilizingScheduleId);

            var R = UIPersistence.GetPlants(State.Id, null, null).First();
            var vm = R.Item1;
            var wS2 = R.Item2;
            var fS2 = R.Item3;

            Assert.AreEqual(vm.Id, State.Id);
            Assert.AreEqual(vm.Name, State.Name);

            Assert.AreEqual(wS.AggregateId, wS2.Id);
            Assert.AreEqual(fS.AggregateId, fS2.Id);

            Assert.AreEqual(wS.Interval, wS2.Interval);
            Assert.AreEqual(fS.Interval, fS2.Interval);
        }

        [Test]
        public void TestUIProjection2()
        {
            Create();
            var wS = new CreateSchedule(Guid.NewGuid(), 2 * 24 * 3600);
            Handler.Handle(wS);
            var fS = new CreateSchedule(Guid.NewGuid(), 60 * 24 * 3600);
            Handler.Handle(fS);
            Handler.Handle(new SetWateringSchedule(State.Id, wS.AggregateId));
            Handler.Handle(new SetFertilizingSchedule(State.Id, fS.AggregateId));

            Assert.AreEqual(wS.AggregateId, State.WateringScheduleId);
            Assert.AreEqual(fS.AggregateId, State.FertilizingScheduleId);

            //var R = UIPersistence.GetPlants(State.Id, null, null).First();
            var vm = App.CurrentPlants(U).Take(1).Timeout(new TimeSpan(0, 0, 5)).Wait();
            //var wS2 = R.Item2;
            //var fS2 = R.Item3;

            Assert.AreEqual(vm.Id, State.Id);
            Assert.AreEqual(vm.Name, State.Name);

            Assert.AreEqual(wS.AggregateId, vm.WateringSchedule.Id);
            Assert.AreEqual(fS.AggregateId, vm.FertilizingSchedule.Id);

            Assert.AreEqual(wS.Interval, vm.WateringSchedule.Interval);
            Assert.AreEqual(fS.Interval, vm.FertilizingSchedule.Interval);
        }

        [Test]
        public void TestUIProjection3()
        {



            Create();

            var vm = TestUtils.WaitForFirst(App.CurrentPlants(U));

            Assert.IsNull(vm.WateringSchedule);
            Assert.IsNull(vm.FertilizingSchedule);

            var wS = new CreateSchedule(Guid.NewGuid(), 2 * 24 * 3600);
            Handler.Handle(wS);
            Handler.Handle(new SetWateringSchedule(State.Id, wS.AggregateId));
            var fS = new CreateSchedule(Guid.NewGuid(), 60 * 24 * 3600);
            Handler.Handle(fS);
            Handler.Handle(new SetFertilizingSchedule(State.Id, fS.AggregateId));

            Assert.AreEqual(wS.AggregateId, State.WateringScheduleId);
            Assert.AreEqual(fS.AggregateId, State.FertilizingScheduleId);

            //var R = UIPersistence.GetPlants(State.Id, null, null).First();

            //var wS2 = R.Item2;
            //var fS2 = R.Item3;
            Assert.IsNotNull(vm.FertilizingSchedule);
            Assert.IsNotNull(vm.WateringSchedule);

            Assert.AreEqual(vm.Id, State.Id);
            Assert.AreEqual(vm.Name, State.Name);

            Assert.AreEqual(wS.AggregateId, vm.WateringSchedule.Id);
            Assert.AreEqual(fS.AggregateId, vm.FertilizingSchedule.Id);

            Assert.AreEqual(wS.Interval, vm.WateringSchedule.Interval);
            Assert.AreEqual(fS.Interval, vm.FertilizingSchedule.Interval);
        }


        [Test]
        public void TestPlantViewModelChangeName()
        {
            var vm = Create();
            var newName = "Jore";
            App.HandleCommand(new SetName(State.Id, newName));
            Assert.AreEqual(newName, vm.Name);


        }

        [Test]
        [Ignore("Not implemented")]
        public void TestPlantViewModelGetsWateringSchedule()
        {

        }

        [Test]
        [Ignore("Not implemented")]
        public void TestPlantViewModelGetsFertilizingSchedule()
        {

        }

        [Test]
        [Ignore("Not implemented")]
        public void TestPlantViewModelGetsCurrentActions()
        {

        }

        [Test]
        [Ignore("Not implemented")]
        public void TestPlantViewModelGetsFutureActions()
        {

        }

    }
}
