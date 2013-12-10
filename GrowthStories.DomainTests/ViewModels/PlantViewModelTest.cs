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
using System.Collections.Generic;


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

            Assert.AreEqual(wS.Interval, (long)vm.WateringSchedule.Interval.Value.TotalSeconds);
            Assert.AreEqual(fS.Interval, (long)vm.FertilizingSchedule.Interval.Value.TotalSeconds);
        }

        [Test]
        public void TestUIProjection3()
        {



            Create();

            var vm = TestUtils.WaitForFirst(App.CurrentPlants(U));

            Assert.IsNull(vm.WateringSchedule.Id);
            Assert.IsNull(vm.FertilizingSchedule.Id);

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

            Assert.AreEqual(wS.Interval, (long)vm.WateringSchedule.Interval.Value.TotalSeconds);
            Assert.AreEqual(fS.Interval, (long)vm.FertilizingSchedule.Interval.Value.TotalSeconds);
        }


        [Test]
        public void TestPlantViewModelChangeName()
        {
            var vm = Create();
            var newName = "Jore";
            var plant = (Plant)Handler.Handle(new SetName(State.Id, newName));
            Assert.AreEqual(newName, plant.State.Name);
            Assert.AreEqual(newName, vm.Name);
        }

        [Test]
        public void TestPlantViewModelChangeSpecies()
        {
            var vm = Create();
            var newSpecies = "Orkidea";
            Assert.AreNotEqual(newSpecies, vm.Species);
            var plant = (Plant)Handler.Handle(new SetSpecies(State.Id, newSpecies));
            Assert.AreEqual(newSpecies, plant.State.Species);
            Assert.AreEqual(newSpecies, vm.Species);
        }

        [Test]
        public void TestPlantViewModelChangeTags()
        {
            var vm = Create();
            var newTags = new HashSet<string>() { "tag1", "tag2" };
            Assert.IsFalse(newTags.SetEquals(vm.Tags));
            var plant = (Plant)Handler.Handle(new SetTags(State.Id, newTags));
            Assert.IsTrue(newTags.SetEquals(plant.State.Tags));
            Assert.IsTrue(newTags.SetEquals(vm.Tags));
        }

        [Test]
        public void TestPlantViewModelChangeWateringSchedule()
        {
            var vm = Create();
            var wSId = new HashSet<string>() { "tag1", "tag2" };
            var scheduleId = Guid.NewGuid();
            var schedule = (Schedule)Handler.Handle(new CreateSchedule(scheduleId, 1));
            Assert.IsNull(vm.WateringSchedule.Id);
            var plant = (Plant)Handler.Handle(new SetWateringSchedule(State.Id, scheduleId));
            Assert.AreEqual(scheduleId, plant.State.WateringScheduleId.Value);
            Assert.AreEqual(scheduleId, vm.WateringSchedule.Id.Value);

        }

        [Test]
        public void TestPlantViewModelSetSchedules()
        {
            var wS = new ScheduleViewModel(new ScheduleState(), ScheduleType.WATERING, App) { Interval = TimeSpan.FromSeconds(2) };
            IPlantViewModel vm = Create(wS);

            Assert.AreEqual(wS.Interval, vm.WateringSchedule.Interval);

            var fS = new ScheduleViewModel(new ScheduleState(), ScheduleType.FERTILIZING, App) { Interval = TimeSpan.FromSeconds(3) };
            vm = Create(null, fS);

            Assert.AreEqual(fS.Interval, vm.FertilizingSchedule.Interval);

            vm = Create(wS, fS);
            Assert.AreEqual(wS.Interval, vm.WateringSchedule.Interval);
            Assert.AreEqual(fS.Interval, vm.FertilizingSchedule.Interval);

        }

        [Test]
        public void TestPlantViewModelChangeFertilizingSchedule()
        {
            var vm = Create();
            var wSId = new HashSet<string>() { "tag1", "tag2" };
            var scheduleId = Guid.NewGuid();
            var schedule = (Schedule)Handler.Handle(new CreateSchedule(scheduleId, 1));
            Assert.IsNull(vm.WateringSchedule.Id);
            var plant = (Plant)Handler.Handle(new SetFertilizingSchedule(State.Id, scheduleId));
            Assert.AreEqual(scheduleId, plant.State.FertilizingScheduleId.Value);
            Assert.AreEqual(scheduleId, vm.FertilizingSchedule.Id.Value);

        }


        [Test]
        public void TestPlantViewModelChangePhoto()
        {
            var vm = Create();
            var newPhoto = new Photo()
            {
                BlobKey = "dfgdfg",
                FileName = "sdfsdf",
                LocalFullPath = "sdfsf"
            };
            Guid plantActionId = Guid.NewGuid();
            Assert.AreNotEqual(newPhoto, vm.Photo);
            var plant = (Plant)Handler.Handle(new SetProfilepicture(State.Id, newPhoto, plantActionId));
            Assert.AreEqual(newPhoto.BlobKey, plant.State.Profilepicture.BlobKey);
            Assert.AreEqual(newPhoto.LocalFullPath, plant.State.Profilepicture.LocalFullPath);
            Assert.AreEqual(newPhoto.FileName, plant.State.Profilepicture.FileName);
            Assert.AreEqual(plantActionId, plant.State.ProfilepictureActionId);

            Assert.AreEqual(newPhoto.BlobKey, vm.Photo.BlobKey);
            Assert.AreEqual(newPhoto.LocalFullPath, vm.Photo.LocalFullPath);
            Assert.AreEqual(newPhoto.FileName, vm.Photo.FileName);
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
