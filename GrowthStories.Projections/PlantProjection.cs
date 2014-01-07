using Growthstories.Core;
using Growthstories.Domain.Entities;
using Growthstories.Domain.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.UI
{
    public class PlantProjection : EventHandlerBase, IEventHandler<PlantCreated>
    {



        private readonly IUIPersistence Persistence;
        private readonly IGSRepository Repository;

        public PlantProjection(IUIPersistence persistence, IGSRepository repository)
        {
            if (persistence == null)
                throw new ArgumentNullException("uipersistence");
            this.Persistence = persistence;
            this.Repository = repository;
            //PlantNames.Add("Jore");
        }

        //public void Handle(PlantCreated @event)
        //{
        //    this.Persistence.PersistPlant(@event);

        //}



        //public PlantState LoadWithId(Guid Id)
        //{
        //    return ((Plant)Repository.GetById(Id)).State;
        //}

        //public IEnumerable<PlantState> LoadWithUserId(Guid UserId)
        //{

        //    foreach (var c in Persistence.UserPlants(UserId))
        //        yield return ((Plant)Repository.GetById(c.EntityId)).State;
        //}

        //public static string testPic1 = @"/Assets/Icons/appbar.add.png";
        //public static string testPic2 = @"/Assets/Icons/appbar.check.png";


        //public IEnumerable<PlantState> FakeLoadWithUserId(Guid guid)
        //{
        //    return new PlantState[] {
        //        //new PlantState(new PlantCreated(new CreatePlant(Guid.NewGuid(),"Jore",guid) {Profilepicture=testPic1})),
        //        //new PlantState(new PlantCreated(new CreatePlant(Guid.NewGuid(),"Kari",guid) {Profilepicture=testPic2}))
        //    };
        //}
    }
}
