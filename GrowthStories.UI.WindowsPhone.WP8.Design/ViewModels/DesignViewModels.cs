
using Growthstories.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Growthstories.UI.ViewModel
{
    public sealed class GardenPivotViewModelDesign
    {
        public List<PlantViewModelDesign> Plants { get; set; }


        public GardenPivotViewModelDesign()
        {
            this.Plants = new List<PlantViewModelDesign>();
            Plants.Add(this.CreatePlant("Sepi"));
            Plants.Add(this.CreatePlant("Jare"));
            Plants.Add(this.CreatePlant("Kari"));
        }

        private PlantViewModelDesign CreatePlant(string p)
        {
            return new PlantViewModelDesign(p, "Aloe Vera");
        }
    }

    public sealed class PlantViewModelDesign
    {

        private readonly PlantStateDesign State;


        public List<PlantActionViewModelDesign> Actions
        {
            get
            {
                return new List<PlantActionViewModelDesign>()
                {

                };
            }
        }

        public PlantViewModelDesign()
            : base()
        {
            this.State = new PlantStateDesign()
            {
                Name = "Jore",
                Species = "Aloe Vera"
            };
        }

        public PlantViewModelDesign(string name, string species)
            : base()
        {
            this.State = new PlantStateDesign()
            {
                Name = name,
                Species = species
            };
        }

        public Guid Id
        {
            get { return Guid.NewGuid(); }
        }

        public Guid UserId
        {
            get { return Guid.NewGuid(); }
        }

        public string Name
        {
            get { return State.Name; }
        }

        public string Header
        {
            get { return State.Name; }
        }

        public string Species
        {
            get { return State.Species; }
        }



        public Photo Photo
        {
            get { return default(Photo); }
        }



        public string PageTitle
        {
            get { return "Garden"; }
        }

        public string UrlPathSegment
        {
            get { return "sdfsdf"; }
        }


    }



    public sealed class PlantActionViewModelDesign
    {

    }

    public sealed class PlantStateDesign
    {
        public string Name { get; set; }
        public string Species { get; set; }
    }


}
