using EventStore;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public class SynchronizerPipelineHook : IPipelineHook
    {

        private readonly long Offset;


        public SynchronizerPipelineHook(long offset)
        {
            Offset = offset;
        }

        public Commit Select(Commit committed)
        {
            return committed;
        }

        public bool PreCommit(Commit attempt)
        {
            if (attempt == null)
            {
                return true;
            }
            try
            {
                foreach (var @event in attempt.Events.Select(msg => (IEvent)msg.Body))
                {

                }
            }
            catch (InvalidCastException) { }


            return true;
        }

        public void PostCommit(Commit committed)
        {
            //if (committed != null)
            //    this.scheduler.ScheduleDispatch(committed);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
