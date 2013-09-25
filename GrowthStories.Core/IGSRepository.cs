using CommonDomain;
using CommonDomain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Core
{
    public interface IGSRepository : IDisposable
    {

        IGSAggregate GetById(Guid id);
        //TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IGSAggregate;
        void PlayById(IGSAggregate aggregate, Guid id);
        void Save(IGSAggregate aggregate);
        void ClearCaches();
    }


}
