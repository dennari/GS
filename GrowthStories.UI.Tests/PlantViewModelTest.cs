using Growthstories.Domain.Messaging;
using Growthstories.Domain;
using Growthstories.UI.ViewModel;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading;
using Growthstories.Domain.Entities;
using Growthstories.Sync;


namespace Growthstories.UI.Tests
{


    public class PlantViewModelTest : ViewModelTestBase
    {



        [Test]
        public void TestPlantViewModel()
        {
            var plant = new CreatePlant(Guid.NewGuid(), "Jore", Ctx.Id);
            Bus.SendCommand(plant);
            Bus.SendCommand(new AddPlant(Ctx.GardenId, plant.AggregateId, Ctx.Id, plant.Name));

            var measurement = new CreatePlantAction(Guid.NewGuid(), Ctx.Id, plant.AggregateId, PlantActionType.MEASURED, "new measurement")
            {
                MeasurementType = MeasurementType.LENGTH,
                Value = 26.0
            };
            Bus.SendCommand(measurement);

            var change = new SetPlantActionProperty(measurement.AggregateId)
            {
                MeasurementType = MeasurementType.LENGTH,
                Value = 57.9
            };

            Bus.SendCommand(change);

            var vm = new PlantViewModel(
                ((Plant)Repository.GetById(plant.AggregateId)).State,
                this.App
             );

            Assert.AreEqual(1, vm.Actions.Count);


            Assert.IsInstanceOf<PlantMeasureViewModel>(vm.Actions[0]);
            var result = (PlantMeasureViewModel)vm.Actions[0];
            Assert.AreEqual(measurement.AggregateId, result.PlantActionId);
            Assert.AreEqual(measurement.MeasurementType, result.Series.Type);
            Assert.AreEqual(change.Value, result.Value.Value);

            var watering = new CreatePlantAction(Guid.NewGuid(), Ctx.Id, plant.AggregateId, PlantActionType.WATERED, "new watering");
            Bus.SendCommand(watering);

            Assert.AreEqual(2, vm.Actions.Count);


            Assert.IsInstanceOf<PlantWaterViewModel>(vm.Actions[0]);
            var result2 = (PlantWaterViewModel)vm.Actions[0];
            Assert.AreEqual(watering.AggregateId, result2.PlantActionId);
            Assert.AreEqual(watering.Note, result2.Note);

            var wateringPropSet = new SetPlantActionProperty(watering.AggregateId)
            {
                Note = "changed note"
            };
            Bus.SendCommand(wateringPropSet);

            Assert.AreEqual(wateringPropSet.Note, result2.Note);
        }

    }
}
