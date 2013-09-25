using Growthstories.Domain.Entities;
using Growthstories.Core;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{



    #region Schedule

    //[DTOObject(DTOType.createSchedule)]
    public class ScheduleCreated : EventBase, ICreateEvent
    {
        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType == null ? _AggregateType = typeof(Schedule) : _AggregateType; }
        }

        [JsonProperty]
        public Guid UserId { get; private set; }

        [JsonProperty]
        public long Interval { get; private set; }

        protected ScheduleCreated() { }
        public ScheduleCreated(Guid id, Guid userId, long interval)
            : base(id)
        {
            this.UserId = userId;
            this.Interval = interval;
        }

        public ScheduleCreated(CreateSchedule cmd)
            : this(cmd.EntityId, cmd.UserId, cmd.Interval)
        {
        }

        public override string ToString()
        {
            return string.Format(@"Created Schedule {0}", EntityId);
        }

    }



    #endregion


}

