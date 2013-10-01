﻿using EventStore;
using EventStore.Dispatcher;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI.Persistence
{
    public class ReadModelDispatcher : IDispatchCommits
    {
        private readonly IUIPersistence Store;
        private readonly IGSRepository Repo;

        public ReadModelDispatcher(IUIPersistence store, IGSRepository repo)
        {
            this.Store = store;
            this.Repo = repo;
        }

        public void Dispatch(Commit commit)
        {
            foreach (var e in commit.Events.Select(x => x.Body)
                                .OfType<IEvent>()
                                .Where(x => x is PlantActionCreated || x is PlantActionPropertySet))
            {

                Guid id = default(Guid);
                if (e is PlantActionCreated)
                    id = ((PlantActionCreated)e).PlantId;
                else
                    id = ((PlantActionPropertySet)e).PlantId;

                var state = ((Plant)Repo.GetById(id)).State;
                var actionState = state.PlantActions[e.EntityId];
                Store.PersistAction(actionState);

            }




        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
