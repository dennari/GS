using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
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
       ICommandHandler<CreatePlant>,
       ICommandHandler<AssignAppUser>,
       ICommandHandler<LogOutAppUser>,
       ICommandHandler<SetAuthToken>,
       ICommandHandler<CreateSyncStream>,
       ICommandHandler<BecomeFollower>,
       ICommandHandler<SetSyncStamp>,
       ICommandHandler<Pull>,
       ICommandHandler<Push>,
       ICommandHandler<SchedulePhotoUpload>,
       ICommandHandler<CompletePhotoUpload>
    {
        public void Handle(CreateGSApp command)
        {
            RaiseEvent(new GSAppCreated(command));
        }
        public void Handle(CreatePlant command)
        {
            RaiseEvent(new SyncStreamCreated(command));
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


        public void Handle(SchedulePhotoUpload command)
        {
            RaiseEvent(new PhotoUploadScheduled(command));
        }

        public void Handle(CompletePhotoUpload command)
        {
            RaiseEvent(new PhotoUploadCompleted(command));
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
                if (cmd is CreatePlant)
                    return true;
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
