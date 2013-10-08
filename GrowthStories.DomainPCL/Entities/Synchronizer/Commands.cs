using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    #region User
    public class CreateSynchronizer : AggregateCommand<Synchronizer>
    {

        public CreateSynchronizer(Guid id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Create synchronizer.", AggregateId);
        }

    }

    public class Synchronize : AggregateCommand<Synchronizer>
    {

        public Synchronize(Guid id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Synchronize.");
        }

    }

    #endregion

}

