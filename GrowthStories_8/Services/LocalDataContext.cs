using Growthstories.WP8.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.WP8.Services
{
    public class LocalDataContext : DataContext
    {
        public const string CONN = "Data Source=isostore:/GS.sdf";

        public LocalDataContext()
            : base(CONN)
        {

        }

        public Table<ModelBase> Models;

        private IQueryable<Garden> _gardens;

        public IQueryable<Garden> Gardens
        {
            get
            {
                if (_gardens == null)
                {
                    _gardens = Models.OfType<Garden>();
                }
                return _gardens;
            }
        }

        private IQueryable<Plant> _plants;

        public IQueryable<Plant> Plants
        {
            get
            {
                if (_plants == null)
                {
                    _plants = Models.OfType<Plant>();
                }
                return _plants;
            }
        }



    }
}
