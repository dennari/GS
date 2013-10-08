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

    public class SetAuthToken : AggregateCommand<User>
    {


        public string AccessToken { get; protected set; }
        public int ExpiresIn { get; protected set; }
        public string RefreshToken { get; protected set; }


        public SetAuthToken(Guid id, string accessToken, string refreshToken, int expiresIn)
            : base(id)
        {
            this.AccessToken = accessToken;
            this.ExpiresIn = expiresIn;
            this.RefreshToken = refreshToken;
        }

        public SetAuthToken(Guid id, IAuthToken auth)
            : this(id, auth.AccessToken, auth.RefreshToken, auth.ExpiresIn)
        {

        }

        public override string ToString()
        {
            return string.Format(@"SetAuthToken access: {0}, refresh: {1}, expires {3}, for user {4}.", AccessToken, RefreshToken, ExpiresIn, AggregateId);
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
        public Guid OfUser { get; private set; }

        protected BecomeFollower() { }
        public BecomeFollower(Guid userId, Guid OfUser, Guid relationshipId)
            : base(userId)
        {
            this.OfUser = OfUser;
            this.EntityId = relationshipId;

        }

        public override string ToString()
        {
            return string.Format(@"User {0} wants to become a follower of user {1}.", this.AggregateId, this.OfUser);
        }

    }

    public class RequestFriendship : AggregateCommand<User>
    {
        protected RequestFriendship() { }
        public RequestFriendship(Guid userId, Guid relationshipId)
            : base(userId)
        {

            this.EntityId = relationshipId;

        }

        public override string ToString()
        {
            return string.Format(@"User {0} requests friendship in relationship {1}.", this.AggregateId, this.EntityId);
        }

    }

    public class AcceptFriendship : AggregateCommand<User>
    {
        protected AcceptFriendship() { }
        public AcceptFriendship(Guid userId, Guid relationshipId)
            : base(userId)
        {

            this.EntityId = relationshipId;

        }

        public override string ToString()
        {
            return string.Format(@"User {0} requests friendship in relationship {1}.", this.AggregateId, this.EntityId);
        }

    }


    #endregion

}

