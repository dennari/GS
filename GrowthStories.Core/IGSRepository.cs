using CommonDomain;
using CommonDomain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Core
{
    public interface IGSRepository
    {

        void PlayById(IGSAggregate aggregate, Guid id);
        void Save(IGSAggregate aggregate);
    }
}
