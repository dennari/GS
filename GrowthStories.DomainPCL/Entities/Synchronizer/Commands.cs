using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    #region User
    public class CreateSynchronizer : EntityCommand<Synchronizer>
    {

        public CreateSynchronizer(Guid id)
            : base(id)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Create synchronizer.", EntityId);
        }

    }

    public class Synchronize : EntityCommand<Synchronizer>
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

