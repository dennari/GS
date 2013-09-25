using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{

    public abstract class ScheduleCommand : EntityCommand<Schedule>
    {
        protected ScheduleCommand() { }
        public ScheduleCommand(Guid EntityId) : base(EntityId) { }
    }

    #region Schedule
    public class CreateSchedule : ScheduleCommand, ICreateCommand
    {

        public Guid UserId { get; set; }
        public long Interval { get; set; }


        protected CreateSchedule() { }
        public CreateSchedule(Guid id, Guid userId, long interval)
            : base(id)
        {
            this.UserId = userId;
            this.Interval = interval;
        }

        public override string ToString()
        {
            return string.Format(@"Create Schedule {0}.", EntityId);
        }

    }

    public class DeleteSchedule : ScheduleCommand
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

