using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using System.Linq;
using Growthstories.Domain.Messaging;
using Growthstories.Core;
using EventStore;
using CommonDomain;
using Growthstories.Sync;

namespace Growthstories.Domain.Entities
{


    public class GSApp : AggregateBase<GSAppState, GSAppCreated>, IGSApp,
       ICommandHandler<CreateGSApp>,
       ICommandHandler<CreateUser>,
       ICommandHandler<SetUsername>,
       ICommandHandler<SetEmail>,
       ICommandHandler<SetPassword>,
       ICommandHandler<AddPlant>,
       ICommandHandler<AddGarden>,
       ICommandHandler<AssignAppUser>,
       ICommandHandler<LogOutAppUser>,
       ICommandHandler<SetAuthToken>,
       ICommandHandler<CreateSyncStream>,
       ICommandHandler<BecomeFollower>,
       ICommandHandler<SetSyncStamp>,
       ICommandHandler<Pull>,
       ICommandHandler<Push>,
       ICommandHandler<SchedulePhotoUpload>,
       ICommandHandler<CompletePhotoUpload>,
       ICommandHandler<InternalRegisterAppUser>
    {
        public void Handle(CreateGSApp command)
        {
            RaiseEvent(new GSAppCreated(command));
        }
        public void Handle(AddPlant command)
        {
            //&& this.State.User.Id == cmd.AggregateId
            RaiseEvent(new SyncStreamCreated(command));
        }
        public void Handle(AddGarden command)
        {
            if (this.State != null && this.State.User != null && this.State.User.Id == command.AggregateId && this.State.User.GardenId == default(Guid))
            {
                var copy = new AddGarden(this.Id, command.GardenId);
                RaiseEvent(new GardenAdded(copy));

            }
        }

        public override void Handle(IDeleteCommand cmd)
        {

        }

        public void Handle(PlantAdded e)
        {
            //if (this.State != null && this.State.User != null && this.State.User.Id == e.AggregateId)
            RaiseEvent(new SyncStreamCreated(e.PlantId, PullStreamType.PLANT, e.AggregateId));
        }

        public void Handle(BecameFollower e)
        {
            //if (this.State != null && this.State.User != null && this.State.User.Id == e.AggregateId)
            if (this.State != null && this.State.User != null && this.State.User.Id == e.AggregateId) // don't sync friends of friends
                RaiseEvent(new SyncStreamCreated(e.Target, PullStreamType.USER));
        }

        public void Handle(UnFollowed e)
        {

            if (this.State.SyncStreamDict.ContainsKey(e.Target))
                RaiseEvent(new SyncStreamDeleted(e.Target));
            // also delete the plant-streams
            var streamIds = this.State.SyncStreams.Where(x => x.AncestorId == e.Target).Select(x => x.StreamId).ToArray();
            foreach (var id in streamIds)
                RaiseEvent(new SyncStreamDeleted(id));
        }

        public void Handle(DeleteAggregate e)
        {
            if (this.State.SyncStreamDict.ContainsKey(e.AggregateId))
                RaiseEvent(new SyncStreamDeleted(e.AggregateId));
        }

        public void Handle(GardenAdded e)
        {
            if (this.State != null && this.State.User != null && this.State.User.Id == e.AggregateId && this.State.User.GardenId == default(Guid))
            {
                var copy = new AddGarden(this.Id, e.GardenId);
                RaiseEvent(new GardenAdded(copy));
            }
        }

        public void Handle(CreateUser command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }
        public void Handle(SetUsername command)
        {
            if (command.AggregateId == this.State.User.Id)
            {
                var copy = new SetUsername(this.Id, command.Username);
                RaiseEvent(new UsernameSet(copy));
            }
        }
        public void Handle(SetEmail command)
        {
            if (command.AggregateId == this.State.User.Id)
            {
                var copy = new SetEmail(this.Id, command.Email);
                RaiseEvent(new EmailSet(copy));
            }
        }

        public void Handle(SetPassword command)
        {
            if (command.AggregateId == this.State.User.Id)
            {
                var copy = new SetPassword(this.Id, command.Password);
                RaiseEvent(new PasswordSet(copy));
            }
        }

        public void Handle(BecomeFollower command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }

        public void Handle(UnFollow command)
        {

            RaiseEvent(new SyncStreamDeleted(command.Target));

            // also delete the plant-streams
            var streamIds = this.State.SyncStreams.Where(x => x.AncestorId == command.Target).Select(x => x.StreamId).ToArray();
            foreach (var id in streamIds)
                RaiseEvent(new SyncStreamDeleted(id));


        }

        public void Handle(CreateSyncStream command)
        {
            RaiseEvent(new SyncStreamCreated(command));
        }
        public void Handle(AssignAppUser command)
        {
            RaiseEvent(new AppUserAssigned(command));
        }
        public void Handle(LogOutAppUser command)
        {
            RaiseEvent(new AppUserLoggedOut(command));
        }
        public void Handle(SetAuthToken command)
        {
            RaiseEvent(new AuthTokenSet(command));
        }
        public void Handle(SetSyncStamp command)
        {
            RaiseEvent(new SyncStampSet(command));
        }
        public void Handle(Pull command)
        {
            RaiseEvent(new Pulled(command));
        }
        public void Handle(Push command)
        {
            RaiseEvent(new Pushed(command));
        }

        public void Handle(InternalRegisterAppUser command)
        {
            RaiseEvent(new InternalRegistered(command));
        }

        public void Handle(SchedulePhotoUpload command)
        {
            RaiseEvent(new PhotoUploadScheduled(command));
        }

        public void Handle(CompletePhotoUpload command)
        {
            RaiseEvent(new PhotoUploadCompleted(command));
        }

        public void Handle(AcquireLocation command)
        {
            RaiseEvent(new LocationAcquired(command));
        }

        //public void Handle(CompletePhotoDownload command)
        //{
        //    RaiseEvent(new PhotoDownloadCompleted(command));
        //}

        //public void Handle(PlantActionCreated e)
        //{
        //    if (e != null && e.Type == PlantActionType.PHOTOGRAPHED)
        //        RaiseEvent(new PhotoDownloadScheduled(e));
        //}

        public static bool CanHandle(IMessage cmd, bool isRemote = false)
        {


            if (!isRemote)
            {
                if (cmd is CreateUser)
                    return true;
                if (cmd is SetUsername)
                    return true;
                if (cmd is SetEmail)
                    return true;
                if (cmd is SetPassword)
                    return true;
                if (cmd is BecomeFollower)
                    return true;
                if (cmd is UnFollow)
                    return true;
                if (cmd is AddPlant)
                    return true;
                if (cmd is AddGarden)
                    return true;
                if (cmd is DeleteAggregate)
                    return true;
            }
            else
            {

                if (cmd is PlantAdded)
                    return true;
                if (cmd is GardenAdded)
                    return true;
                if (cmd is BecameFollower)
                    return true;
                if (cmd is UnFollowed)
                    return true;


            }
            //else
            //{
            //    var e = cmd as PlantActionCreated;
            //    if (e != null && e.Type == PlantActionType.PHOTOGRAPHED)
            //        return true;
            //}
            return false;
        }



    }
}
