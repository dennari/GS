using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{



    #region Schedule
    public class CreateSchedule : EntityCommand<User>
    {

        public Guid UserId { get; set; }
        public long Interval { get; set; }


        protected CreateSchedule() { }
        public CreateSchedule(Guid id, Guid userId, long interval)
            : base(id)
        {

            this.UserId = userId;
            this.Interval = interval;
            this.StreamEntityId = userId;
            this.AncestorId = userId;
            this.ParentId = userId;
            //this.StreamAncestorId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Create Schedule {0}.", EntityId);
        }

    }

    public class DeleteSchedule : EntityCommand<User>
    {

        protected DeleteSchedule() { }
        public DeleteSchedule(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete Schedule {0}.", EntityId);
        }

    }



    #endregion


}

