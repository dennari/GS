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

    [DTOObject(DTOType.createIntervalSchedule)]
    public class ScheduleCreated : EventBase, ICreateEvent
    {

        [JsonIgnore]
        private Type _AggregateType;
        [JsonIgnore]
        public Type AggregateType
        {
            get { return _AggregateType ?? (_AggregateType = typeof(Schedule)); }
        }

        [JsonProperty]
        public long Interval { get; private set; }

        protected ScheduleCreated() { }


        public ScheduleCreated(CreateSchedule cmd)
            : base(cmd)
        {
            this.Interval = cmd.Interval;

        }

        public override string ToString()
        {
            return string.Format(@"Created Schedule {0}", EntityId);
        }

        public override void FillDTO(IEventDTO Dto)
        {
            var D = (IAddIntervalScheduleDTO)Dto;
            D.Interval = this.Interval;


            base.FillDTO(D);
            //D.Name = this.Name;
        }

        public override void FromDTO(IEventDTO Dto)
        {
            var D = (IAddIntervalScheduleDTO)Dto;
            this.Interval = D.Interval;
            base.FromDTO(D);
        }

    }



    #endregion


}

