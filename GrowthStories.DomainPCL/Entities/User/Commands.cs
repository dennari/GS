using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;
using Growthstories.Sync;


namespace Growthstories.Domain.Messaging
{


    #region User
    public class CreateUser : AggregateCommand<User>, ICreateCommand
    {

        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Email { get; private set; }


        public CreateUser(Guid id, string username, string password, string email)
            : base(id)
        {

            if (username == null)
                throw new ArgumentNullException();
            if (password == null)
                throw new ArgumentNullException();
            if (email == null)
                throw new ArgumentNullException();

            this.Username = username;
            this.Password = password;
            this.Email = email;
        }

        public override string ToString()
        {
            return string.Format(@"Create user {0}, username {1}, email {2}.", AggregateId, Username, Email);
        }

    }





    public class DeleteUser : AggregateCommand<User>
    {
        protected DeleteUser() { }
        public DeleteUser(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete user {0}.", AggregateId);
        }

    }

    public class AddGarden : AggregateCommand<User>
    {
        public readonly Guid GardenId;
        protected AddGarden() { }
        public AddGarden(Guid userId, Guid gardenId)
            : base(userId)
        {
            //this.EntityId = gardenId;
            this.GardenId = gardenId;

        }

        public override string ToString()
        {
            return string.Format(@"Added garden {0} to user {0}.", EntityId, AggregateId);
        }

    }

    public class BecomeFollower : AggregateCommand<User>
    {
        public Guid Target { get; private set; }

        protected BecomeFollower() { }
        public BecomeFollower(Guid userId, Guid OfUser)
            : base(userId)
        {
            this.Target = OfUser;

        }

        public override string ToString()
        {
            return string.Format(@"User {0} wants to become a follower of user {1}.", this.AggregateId, this.Target);
        }

    }

    public class RequestCollaboration : AggregateCommand<User>
    {
        public Guid Target { get; private set; }

        protected RequestCollaboration() { }
        public RequestCollaboration(Guid userId, Guid target)
            : base(userId)
        {

            this.Target = target;

        }

        public override string ToString()
        {
            return string.Format(@"User {0}: request collaboration with {1}.", this.AggregateId, this.Target);
        }

    }

    public class DenyCollaboration : AggregateCommand<User>
    {
        public Guid Target { get; private set; }

        protected DenyCollaboration() { }
        public DenyCollaboration(Guid userId, Guid target)
            : base(userId)
        {

            this.Target = target;

        }

        public override string ToString()
        {
            return string.Format(@"User {0}: deny collaboration with {1}.", this.AggregateId, this.Target);
        }

    }

    #endregion

}

