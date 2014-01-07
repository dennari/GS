using CommonDomain;
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
        void PlayById(IGSAggregate aggregate, Guid id);
        void Save(IGSAggregate aggregate);
        void Save(IGSAggregate[] aggregates);
        int GetGlobalCommitSequence();

    }




}
