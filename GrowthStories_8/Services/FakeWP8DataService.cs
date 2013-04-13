
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using System;
using Growthstories.WP8.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Growthstories.PCL.Services;

namespace Growthstories.WP8.Services
{

    public class MyDataContext : DataContext
    {
        public const string CONN = "Data Source=isostore:/GS.sdf";

        public MyDataContext()
            : base(CONN)
        {

        }

        public Table<Garden> Gardens;

        public Table<Plant> Plants;

    }





    public class FakeWP8DataService : IDataService
    {



        public async Task<Garden> LoadGarden(User u)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Garden>> LoadGardens(User u)
        {
            throw new NotImplementedException();
        }
    }
}
