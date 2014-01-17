using System;

namespace Growthstories.Core
{
    public interface IGSRepository : IDisposable
    {

        IGSAggregate GetById(Guid id);
        void PlayById(IGSAggregate aggregate, Guid id);
        void Save(IGSAggregate aggregate);
        void Save(IGSAggregate[] aggregates);
        int GetGlobalCommitSequence();

        void ClearCaches();

    }




}
