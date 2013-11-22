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


    public class AddEditPlantViewModelTest : ViewModelTestBase
    {

        protected PlantState State;

        protected IPlantViewModel CreatePlantVM(IScheduleViewModel wateringSchedule = null, IScheduleViewModel fertilizingSchedule = null)
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
        public void TestAddMinimumPlant()
        {


            var vm = new AddEditPlantViewModel(App);

            vm.Name = "Sepi";
            vm.Species = "Aloe v.";

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            var plant = (Plant)Repository.GetById(id);
            Assert.AreEqual(vm.Name, plant.State.Name);
            Assert.AreEqual(vm.Species, plant.State.Species);



        }

        [Test]
        public void TestAddWateringScheduledPlant()
        {


            var vm = new AddEditPlantViewModel(App);

            vm.Name = "Sepi";
            vm.Species = "Aloe v.";

            Assert.IsNull(vm.WateringSchedule.Id);
            Assert.AreEqual(ScheduleType.WATERING, vm.WateringSchedule.Type);
            var interval = TimeSpan.FromSeconds(7200);
            vm.WateringSchedule.Interval = interval;
            var wSID = vm.WateringSchedule.Id;
            Assert.IsNotNull(wSID);

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            var plant = (Plant)Repository.GetById(id);
            Assert.AreEqual(wSID, plant.State.WateringScheduleId);

            var schedule = (Schedule)Repository.GetById(wSID.Value);
            Assert.AreEqual(interval.TotalSeconds, schedule.State.Interval);


        }

        [Test]
        public void TestAddFertilizingScheduledPlant()
        {


            var vm = new AddEditPlantViewModel(App);

            vm.Name = "Sepi";
            vm.Species = "Aloe v.";

            Assert.IsNull(vm.FertilizingSchedule.Id);
            Assert.AreEqual(ScheduleType.FERTILIZING, vm.FertilizingSchedule.Type);
            var interval = TimeSpan.FromSeconds(7200);
            vm.FertilizingSchedule.Interval = interval;
            var wSID = vm.FertilizingSchedule.Id;
            Assert.IsNotNull(wSID);

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            var plant = (Plant)Repository.GetById(id);
            Assert.AreEqual(wSID, plant.State.FertilizingScheduleId);

            var schedule = (Schedule)Repository.GetById(wSID.Value);
            Assert.AreEqual(interval.TotalSeconds, schedule.State.Interval);


        }

        [Test]
        public void TestAddCopyWateringScheduledPlant()
        {

            var garden = App.Resolver.GetService<IGardenViewModel>();
            Assert.AreEqual(U.GardenId, garden.Id);
            Assert.AreEqual(0, garden.Plants.Count);

            var plantVM = CreatePlantVM();
            var schedule = (Schedule)Handler.Handle(new CreateSchedule(Guid.NewGuid(), 23));
            Handler.Handle(new SetWateringSchedule(plantVM.Id, schedule.Id));
            Assert.AreEqual(plantVM.WateringSchedule.Id, schedule.Id);

            //Handler.Handle()
            Assert.AreEqual(1, garden.Plants.Count);



            var vm = new AddEditPlantViewModel(App);


            Assert.AreEqual(1, vm.WateringSchedule.OtherSchedules.Count());
            var schedule2 = ((IList<Tuple<IPlantViewModel, IScheduleViewModel>>)vm.WateringSchedule.OtherSchedules)[0].Item2;
            Assert.AreEqual(schedule.Id, schedule2.Id.Value);
            Assert.AreEqual(schedule.State.Interval, schedule2.Interval.Value.TotalSeconds);
        }


        [Test]
        public void TestAddCopyFertilizingScheduledPlant()
        {

            var garden = App.Resolver.GetService<IGardenViewModel>();
            Assert.AreEqual(U.GardenId, garden.Id);
            Assert.AreEqual(0, garden.Plants.Count);

            var plantVM = CreatePlantVM();
            var schedule = (Schedule)Handler.Handle(new CreateSchedule(Guid.NewGuid(), 23));
            Handler.Handle(new SetFertilizingSchedule(plantVM.Id, schedule.Id));
            Assert.AreEqual(plantVM.FertilizingSchedule.Id, schedule.Id);

            //Handler.Handle()
            Assert.AreEqual(1, garden.Plants.Count);



            var vm = new AddEditPlantViewModel(App);


            Assert.AreEqual(1, vm.FertilizingSchedule.OtherSchedules.Count());
            var schedule2 = ((IList<Tuple<IPlantViewModel, IScheduleViewModel>>)vm.FertilizingSchedule.OtherSchedules)[0].Item2;
            Assert.AreEqual(schedule.Id, schedule2.Id.Value);
            Assert.AreEqual(schedule.State.Interval, schedule2.Interval.Value.TotalSeconds);
        }

        [Test]
        public void TestEditName()
        {

            var garden = App.Resolver.GetService<IGardenViewModel>();
            Assert.AreEqual(U.GardenId, garden.Id);
            Assert.AreEqual(0, garden.Plants.Count);

            string name = "Sepi";
            string newName = "Seppo";
            string species = "Aloe";
            AddEditPlantViewModel vm = new AddEditPlantViewModel(App);

            vm.Name = name;
            vm.Species = species;

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            Assert.AreEqual(1, garden.Plants.Count());
            var pvm = garden.Plants[0];
            Assert.AreEqual(name, pvm.Name);
            Assert.AreEqual(species, pvm.Species);


            vm = new AddEditPlantViewModel(App, pvm);
            vm.Name = newName;

            Guid newId = TestUtils.WaitForTask(vm.AddTask());
            Assert.AreEqual(id, newId);
            Assert.AreEqual(newName, pvm.Name);



        }

        [Test]
        public void TestEditSpecies()
        {

            var garden = App.Resolver.GetService<IGardenViewModel>();
            Assert.AreEqual(U.GardenId, garden.Id);
            Assert.AreEqual(0, garden.Plants.Count);

            string name = "Sepi";
            string species = "Aloe";
            string newSpecies = "Orchideemus";

            AddEditPlantViewModel vm = new AddEditPlantViewModel(App);

            vm.Name = name;
            vm.Species = species;

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            Assert.AreEqual(1, garden.Plants.Count());
            var pvm = garden.Plants[0];
            Assert.AreEqual(name, pvm.Name);
            Assert.AreEqual(species, pvm.Species);




            vm = new AddEditPlantViewModel(App, pvm);
            vm.Species = newSpecies;

            Guid newId = TestUtils.WaitForTask(vm.AddTask());
            Assert.AreEqual(id, newId);
            Assert.AreEqual(newSpecies, pvm.Species);



        }

        [Test]
        public void TestEditWateringSchedule()
        {

            var garden = App.Resolver.GetService<IGardenViewModel>();
            Assert.AreEqual(U.GardenId, garden.Id);
            Assert.AreEqual(0, garden.Plants.Count);

            string name = "Sepi";
            string species = "Aloe";
            int interval = 23;
            int newInterval = 24;


            AddEditPlantViewModel vm = new AddEditPlantViewModel(App);

            vm.Name = name;
            vm.Species = species;
            vm.WateringSchedule.Interval = TimeSpan.FromSeconds(interval);
            var wSID = vm.WateringSchedule.Id.Value;

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            Assert.AreEqual(1, garden.Plants.Count());
            var pvm = garden.Plants[0];
            Assert.AreEqual(name, pvm.Name);
            Assert.AreEqual(species, pvm.Species);
            Assert.AreEqual(interval, (int)pvm.WateringSchedule.Interval.Value.TotalSeconds);



            vm = new AddEditPlantViewModel(App, pvm);
            vm.WateringSchedule.Interval = TimeSpan.FromSeconds(newInterval);

            Guid newId = TestUtils.WaitForTask(vm.AddTask());
            Assert.AreEqual(id, newId);
            Assert.AreEqual(newInterval, (int)pvm.WateringSchedule.Interval.Value.TotalSeconds);



        }

        [Test]
        public void TestEditFertilizingSchedule()
        {

            var garden = App.Resolver.GetService<IGardenViewModel>();
            Assert.AreEqual(U.GardenId, garden.Id);
            Assert.AreEqual(0, garden.Plants.Count);

            string name = "Sepi";
            string species = "Aloe";
            int interval = 23;
            int newInterval = 24;


            AddEditPlantViewModel vm = new AddEditPlantViewModel(App);

            vm.Name = name;
            vm.Species = species;
            vm.FertilizingSchedule.Interval = TimeSpan.FromSeconds(interval);
            var wSID = vm.FertilizingSchedule.Id.Value;

            Guid id = TestUtils.WaitForTask(vm.AddTask());

            Assert.AreEqual(1, garden.Plants.Count());
            var pvm = garden.Plants[0];
            Assert.AreEqual(name, pvm.Name);
            Assert.AreEqual(species, pvm.Species);
            Assert.AreEqual(interval, (int)pvm.FertilizingSchedule.Interval.Value.TotalSeconds);



            vm = new AddEditPlantViewModel(App, pvm);
            vm.FertilizingSchedule.Interval = TimeSpan.FromSeconds(newInterval);

            Guid newId = TestUtils.WaitForTask(vm.AddTask());
            Assert.AreEqual(id, newId);
            Assert.AreEqual(newInterval, (int)pvm.FertilizingSchedule.Interval.Value.TotalSeconds);



        }



    }
}
