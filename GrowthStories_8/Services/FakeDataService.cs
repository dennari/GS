using Growthstories.WP8.Domain.Entities;
using Growthstories.PCL.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Parameters;


namespace Growthstories.WP8.Services
{
    public class FakeDataService : IDataService
    {
        private IKernel kernel;

        private const string TestPhotoPath = "/Assets/rose.jpg";

        public FakeDataService(IKernel k)
        {
            kernel = k;
        }

        public async Task<Garden> LoadGarden(User u)
        {
            var photoBase = "/Assets/Photos/{0}";
            var garden = kernel.Get<Garden>();
            garden.Notifications.Add(new Notification()
            {
                Icon = "/Assets/gs_ikoneita_05.png",
                Msg = "Mist Sepi"
            });
            garden.Notifications.Add(new Notification()
            {
                Icon = "/Assets/gs_ikoneita_03.png",
                Msg = "Measure Kari"
            });
            garden.Notifications.Add(new Notification()
            {
                Icon = "/Assets/gs_ikoneita_11.png",
                Msg = "Change Jori's soil"
            });


            Plant plant = this.kernel.Get<Plant>();
            plant.Name = "Sepi";
            plant.Genus = "Aloe Vera";
            plant.ProfilePicturePath = string.Format(photoBase, "3547545731_efd5a20872_o.jpg");
            garden.Plants.Add(plant);

            plant = this.kernel.Get<Plant>();
            plant.Name = "Jore";
            plant.Genus = "Phalaenopsis";
            plant.ProfilePicturePath = string.Format(photoBase, "4256637373_e828423506_o.jpg");
            var photoUri = new Uri(string.Format(photoBase, "3547545731_efd5a20872_o.jpg"), UriKind.Relative);
            PlantAction action = new PhotoAction(plant, photoUri);
            action.CreatedAt = action.CreatedAt.Value.AddDays(-2);
            action.ModifiedAt = action.CreatedAt;
            action.Note = "You think water moves fast? You should see ice. It moves like it has a mind. Like it knows it killed the world once and got a taste for murder. After the avalanche, it took us a week to climb out. Now, I don't know exactly when we turned on each other, but I know that seven of us survived the slide... and only five made it out. Now we took an oath, that I'm breaking now. We said we'd say it was the snow that killed the other two, but it wasn't. Nature is lethal but it doesn't hold a candle to man.";
            plant.Actions.Add(action);
            action = new WateringAction(plant);
            action.CreatedAt = action.CreatedAt.Value.AddDays(-4);
            action.ModifiedAt = action.CreatedAt;
            action.Note = "You think water moves fast? You should see ice. It moves like it has a mind. Like it knows it killed the world once and got a taste for murder. After the avalanche, it took us a week to climb out. Now, I don't know exactly when we turned on each other, but I know that seven of us survived the slide... and only five made it out. Now we took an oath, that I'm breaking now. We said we'd say it was the snow that killed the other two, but it wasn't. Nature is lethal but it doesn't hold a candle to man.";

            plant.Actions.Add(action);
            action = new FertilizerAction(plant);
            action.CreatedAt = action.CreatedAt.Value.AddDays(-6);
            action.ModifiedAt = action.CreatedAt;
            action.Note = "You think water moves fast? You should see ice. It moves like it has a mind. Like it knows it killed the world once and got a taste for murder. After the avalanche, it took us a week to climb out. Now, I don't know exactly when we turned on each other, but I know that seven of us survived the slide... and only five made it out. Now we took an oath, that I'm breaking now. We said we'd say it was the snow that killed the other two, but it wasn't. Nature is lethal but it doesn't hold a candle to man.";

            plant.Actions.Add(action);
            action = new PhotoAction(plant, photoUri);
            action.CreatedAt = action.CreatedAt.Value.AddDays(-8);
            action.ModifiedAt = action.CreatedAt;
            action.Note = "You think water moves fast? You should see ice. It moves like it has a mind. Like it knows it killed the world once and got a taste for murder. After the avalanche, it took us a week to climb out. Now, I don't know exactly when we turned on each other, but I know that seven of us survived the slide... and only five made it out. Now we took an oath, that I'm breaking now. We said we'd say it was the snow that killed the other two, but it wasn't. Nature is lethal but it doesn't hold a candle to man.";

            plant.Actions.Add(action);
            garden.Plants.Add(plant);

            plant = this.kernel.Get<Plant>();
            plant.Name = "Kari";
            plant.Genus = "Cattleya";
            plant.ProfilePicturePath = string.Format(photoBase, "7530054972_6e987e8540_o.jpg");
            garden.Plants.Add(plant);

            plant = this.kernel.Get<Plant>();
            plant.Name = "Timo";
            plant.Genus = "Ocimum basilicum";
            plant.ProfilePicturePath = string.Format(photoBase, "7568630428_b68d55a317_o.jpg");
            garden.Plants.Add(plant);

            plant = this.kernel.Get<Plant>();
            plant.Name = "Niko";
            plant.Genus = "Mentha spicata";
            plant.ProfilePicturePath = string.Format(photoBase, "7813862886_abaa022b57_o.jpg");
            garden.Plants.Add(plant);


            return garden;


        }

        public T Get<T>()
        {
            throw new NotImplementedException();
        }

        public T Get<T>(params IParameter[] parameters)
        {

            throw new NotImplementedException();

        }

        public EntityChanges<ModelBase> Changes
        {
            get
            {
                throw new NotImplementedException();
            }

        }

        public Task<IList<Garden>> LoadGardens(User u)
        {
            throw new NotImplementedException();
        }
    }
}
