using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{



    #region GSApp
    public class CreateGSApp : AggregateCommand<GSApp>, ICreateCommand
    {



        //protected CreateGSApp() { }
        public CreateGSApp()
            : base(GSAppState.GSAppId)
        {

        }

        public override string ToString()
        {
            return string.Format(@"Create GSApp {0}.", EntityId);
        }

    }

    public class AssignAppUser : AggregateCommand<GSApp>
    {
        public readonly Guid UserId;



        protected AssignAppUser() { }
        public AssignAppUser(Guid userId)
            : base(GSAppState.GSAppId)
        {
            this.UserId = userId;
        }

        public override string ToString()
        {
            return string.Format(@"Assign user {0} to app.", UserId);
        }

    }



    #endregion


}

