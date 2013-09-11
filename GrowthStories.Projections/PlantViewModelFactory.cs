using Growthstories.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public interface IPlantViewModelFactory
    {
        IPlantViewModel Assemble(Guid plantId);
    }

    class PlantViewModelFactory : IPlantViewModelFactory
    {

        public PlantViewModelFactory()
        {

        }

        public IPlantViewModel Assemble(Guid plantId)
        {
            throw new NotImplementedException();
        }
    }
}
