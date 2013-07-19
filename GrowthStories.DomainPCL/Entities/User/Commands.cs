using Growthstories.Domain.Entities;
//using CommonDomain;
using System;
using Growthstories.Core;


namespace Growthstories.Domain.Messaging
{


    #region User
    public class CreateUser : EntityCommand<User>
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
            return string.Format(@"Create user {0}, username {1}, email {2}.", EntityId, Username, Email);
        }

    }

    public class SetAuthToken : EntityCommand<User>
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

        public SetAuthToken(Guid id, Sync.IAuthTokenResponse auth)
            : this(id, auth.AccessToken, auth.RefreshToken, auth.ExpiresIn)
        {

        }

        public override string ToString()
        {
            return string.Format(@"SetAuthToken access: {0}, refresh: {1}, expires {3}, for user {4}.", AccessToken, RefreshToken, ExpiresIn, EntityId);
        }

    }



    public class DeleteUser : EntityCommand<User>
    {
        protected DeleteUser() { }
        public DeleteUser(Guid id) : base(id) { }

        public override string ToString()
        {
            return string.Format(@"Delete user {0}.", EntityId);
        }

    }

    public class AddGarden : EntityCommand<User>
    {
        public Guid GardenId { get; protected set; }
        protected AddGarden() { }
        public AddGarden(Guid id, Guid gardenId)
            : base(id)
        {
            this.GardenId = gardenId;
        }

        public override string ToString()
        {
            return string.Format(@"Added garden {0} to user {0}.", GardenId, EntityId);
        }

    }

    public class BecomeFollower : EntityCommand<User>
    {
        public Guid OfUser { get; private set; }

        protected BecomeFollower() { }
        public BecomeFollower(Guid id, Guid OfUser)
            : base(id)
        {
            this.OfUser = OfUser;
        }

        public override string ToString()
        {
            return string.Format(@"User {0} wants to become a follower of user {1}.", this.EntityId, this.OfUser);
        }

    }

    public class Comment : ActionBase
    {


        protected Comment() { }
        public Comment(Guid userId, Guid plantId, string note)
            : base(userId, plantId, note) { }


        public override string ToString()
        {
            return string.Format("Comment '{0}' on plant {1} (user {2})", Note, PlantId, EntityId);
        }


    }

    public class Water : ActionBase
    {


        protected Water() { }
        public Water(Guid userId, Guid plantId, string note)
            : base(userId, plantId, note) { }


        public override string ToString()
        {
            return string.Format("Water plant {0} (user {1})", PlantId, EntityId);
        }


    }

    public class Fertilize : ActionBase
    {


        protected Fertilize() { }
        public Fertilize(Guid userId, Guid plantId, string note)
            : base(userId, plantId, note) { }


        public override string ToString()
        {
            return string.Format("Fertilize plant {0} (user {1})", PlantId, EntityId);
        }


    }

    public class Photograph : ActionBase
    {


        public Uri Uri { get; private set; }

        protected Photograph() { }
        public Photograph(Guid userId, Guid plantId, string note, Uri uri)
            : base(userId, plantId, note)
        {
            this.Uri = uri;
        }


        public override string ToString()
        {
            return string.Format("Photograph plant {0} (user {1})", PlantId, EntityId);
        }


    }

    #endregion

}

