using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{



    #region Schedule
    public class CreateSchedule : AggregateCommand<Schedule>, ICreateMessage
    {

        public long Interval { get; set; }


        protected CreateSchedule() { }
        public CreateSchedule(Guid id, long interval)
            : base(id)
        {

            this.Interval = interval;
        }

        public override string ToString()
        {
            return string.Format(@"Create Schedule {0}.", EntityId);
        }

    }

    public class DeleteSchedule : AggregateCommand<Schedule>
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

