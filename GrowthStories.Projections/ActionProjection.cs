using Growthstories.Core;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public class ActionProjection : ProjectionBase,
        IEventHandler<Commented>,
        IEventHandler<Fertilized>,
        IEventHandler<Watered>,
        IEventHandler<Photographed>
    {

        private readonly IDictionary<Guid, IList<ActionBase>> _Actions = new Dictionary<Guid, IList<ActionBase>>();
        private readonly IUIPersistence Persistence;
        public IDictionary<Guid, IList<ActionBase>> Actions { get { return _Actions; } }

        public ActionProjection(IUIPersistence persistence)
        {
            if (persistence == null)
                throw new ArgumentNullException("uipersistence");
            this.Persistence = persistence;
        }


        public void Handle(Commented @event)
        {
            HandleAction(@event);
        }

        public void Handle(Fertilized @event)
        {
            HandleAction(@event);
        }

        public void Handle(Watered @event)
        {
            HandleAction(@event);
        }

        public void Handle(Photographed @event)
        {
            HandleAction(@event);
        }

        protected void HandleAction(ActionBase @event)
        {
            IList<ActionBase> list = null;
            //try
            //{
            //    list = Actions[@event.PlantId];
            //}
            //catch (Exception)
            //{
            //    list = new List<ActionBase>();
            //}
            if (!Actions.TryGetValue(@event.PlantId, out list))
                Actions[@event.PlantId] = list = new List<ActionBase>();

            list.Add(@event);
            //Actions[@event.PlantId] = list;
            Persistence.PersistAction(@event);
        }


        public IEnumerable<ActionBase> LoadWithPlantId(Guid PlantId)
        {
            return Persistence.PlantActions(PlantId);
        }

        public IEnumerable<ActionBase> LoadWithUserId(Guid UserId)
        {
            return Persistence.UserActions(UserId);
        }
    }

}
