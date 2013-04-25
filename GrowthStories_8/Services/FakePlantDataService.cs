using Growthstories.WP8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Services
{
    public class FakePlantDataService : IPlantDataService
    {
        public async Task<IList<PlantData>> LoadPlantDataAsync(string genus)
        {
            List<PlantData> r = new List<PlantData>();
            r.Add(new PlantData());
            r.Add(new PlantData());
            return r;
        }
    }
}
