
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

        public string Username { get; set; }

        public GardenPivotViewModelDesign()
        {
            this.Plants = new List<PlantViewModelDesign>();
            this.Username = "Lauriii";
            Plants.Add(this.CreatePlant("Sepi"));
            Plants.Add(this.CreatePlant("Jare"));
            Plants.Add(this.CreatePlant("Kari"));
        }

        private PlantViewModelDesign CreatePlant(string p)
        {
            return new PlantViewModelDesign(p, "Aloe Vera");
        }
    }

    public class PlantViewModelDesign
    {

        private readonly PlantStateDesign State;


        public List<PlantActionViewModelDesign> Actions { get; set; }


        public PlantViewModelDesign()
            : base()
        {
            this.State = new PlantStateDesign()
            {
                Name = "Jore",
                Species = "Aloe Vera"
            };

            this.Actions = new List<PlantActionViewModelDesign>()
            {
                new PlantPhotoViewModel(@"/TestData/flowers-from-the-conservatory.jpg"),                
                new PlantWaterViewModel(),
                new PlantMeasureViewModel(),
                new PlantPhotoViewModel(),
                new PlantFertilizeViewModel(),
                new PlantCommentViewModel()
            };

            this.Photo = new Photo()
            {
                LocalFullPath = @"/TestData/517e100d782a828894.jpg",
                LocalUri = @"/TestData/517e100d782a828894.jpg"
            };
        }

        public PlantViewModelDesign(string name, string species)
            : this()
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



        public Photo Photo { get; set; }


        public string PageTitle
        {
            get { return "Garden"; }
        }

        public string UrlPathSegment
        {
            get { return "sdfsdf"; }
        }


    }




    public sealed class PlantStateDesign
    {
        public string Name { get; set; }
        public string Species { get; set; }
    }


    public class PlantPivotViewModelDesign : PlantViewModelDesign
    {

        public IPlantActionViewModel SelectedItem { get; set; }
        public PlantActionType? Filter { get; set; }


        protected List<PlantActionViewModelDesign> _FilteredActions;
        public List<PlantActionViewModelDesign> FilteredActions
        {
            get
            {
                if (_FilteredActions == null)
                    _FilteredActions = !Filter.HasValue ? Actions : Actions.Where(x => x.ActionType == Filter.Value).ToList();
                return _FilteredActions;
            }
        }


        public PlantPivotViewModelDesign()
            : base()
        {
            //Filter = PlantActionType.PHOTOGRAPHED;
            //SelectedItem = FilteredActions[0];
        }
    }

    public class PlantPhotoPivotViewModelDesign : PlantPivotViewModelDesign
    {
        public PlantPhotoPivotViewModelDesign()
            : base()
        {
            Filter = PlantActionType.PHOTOGRAPHED;
        }
    }




}
