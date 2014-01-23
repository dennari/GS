
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Enough.Async;
using Growthstories.Core;
using Growthstories.Domain;
using Growthstories.Domain.Messaging;
using ReactiveUI;

namespace Growthstories.Sync
{


    public class SynchronizerService : ISynchronizer, IEnableLogger
    {
        //private int AutoSyncCount = 0;
        //private bool AutoSyncEnabled = true;

        //private Task<ISyncInstance> currentSynchronize;
        //private Task<Tuple<AllSyncResult, GSStatusCode?>> currentSyncAll;



        //private readonly Subject<IGSAppState> AutoSyncSubject = new Subject<IGSAppState>();

        private readonly IDispatchCommands Handler;
        private readonly IRequestFactory RequestF;
        private readonly IMessageBus MessageBus;
        private readonly IUserService AuthService;

        //private readonly TimeSpan ThrottleInterval;

        public SynchronizerService(
            IDispatchCommands handler,
            IRequestFactory requestF,
            IMessageBus messageBus,
            IUserService authService,
            bool IsAutoSyncEnabled = false,
            int throttleInterval = 10
            )
        {
            this.Handler = handler;
            this.RequestF = requestF;
            this.MessageBus = messageBus;
            this.AuthService = authService;
            this.IsAutoSyncEnabled = IsAutoSyncEnabled;
            this.ThrottleInterval = throttleInterval;

        }

        private AsyncLock _SyncLock = new AsyncLock();
        private bool IsAutoSyncEnabled;
        private int ThrottleInterval;

        public async Task<IDisposable> AcquireLock()
        {
            return await _SyncLock.LockAsync();
        }

        private bool IsAutosyncTemporarilyDisabled = false;
        public IDisposable DisableAutoSync()
        {
            //var current = this.IsAutoSyncEnabled;
            if (!IsAutoSyncEnabled || IsAutosyncTemporarilyDisabled)
                return Disposable.Empty;
            this.Log().Info("Disabling autosync");
            IsAutosyncTemporarilyDisabled = true;
            return Disposable.Create(() =>
            {
                this.Log().Info("Re-enabling autosync");
                this.IsAutosyncTemporarilyDisabled = false;
            });
        }



        // Tries to prepare an authorized user
        //
        //  - Pushes UserCreated if necessary
        //  - Obtains AuthToken if necessary
        //  
        //  ( May add later auth check with server)
        //
        public async Task<GSStatusCode> PrepareAuthorizedUser(SyncHead head)
        {


            // if we have not yet pushed the CreateUser event,
            // do that before obtaining auth token
            var res = RequestF.GetNextPushEvent(head);

            if (res.Item1 is UserCreated)
            {
                var s = await PushCreateUser(head);

                if (s.Status != SyncStatus.OK)
                {
                    // this should not happen
                    this.Log().Warn("failed to push CreateUser for user id " + res.Item1.AggregateId);
                    return GSStatusCode.FAIL;
                }
            }

            if (AuthService.CurrentUser.AccessToken == null)
            {
                var authResponse = await AuthService.AuthorizeUser();
                return authResponse.StatusCode;
            }

            return GSStatusCode.OK;
        }


        public async Task<ISyncInstance> CreateSyncRequest(IGSAppState appState)
        {
            var code = await PrepareAuthorizedUser(appState.SyncHead);
            if (code != GSStatusCode.OK)
            {
                return new SyncInstance(SyncStatus.AUTH_ERROR);
            }

            var syncStreams = appState.SyncStreams.ToArray();
            var s = new SyncInstance
            (
                RequestF.CreatePullRequest(syncStreams),
                RequestF.CreatePushRequest(appState.SyncHead),
                appState.PhotoUploads.Values.Select(x => RequestF.CreatePhotoUploadRequest(x)).ToArray(),
                null
            );

            // pullrequest should really never be empty
            if (s.PullReq.IsEmpty && s.PushReq.IsEmpty && s.PhotoUploadRequests.Length == 0)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return new SyncInstance(SyncStatus.PULL_EMPTY_ERROR);
            }

            return s;
        }


        private Task<bool> Prev<T>(Task<T> prev)
        {
            if (prev != null && prev.Status == TaskStatus.Running)
            {
                return Task.Run(async () =>
                {
                    await prev;
                    return true;
                });

            }

            return Task.FromResult(false);
        }


        public async Task<ISyncInstance> Synchronize(IGSAppState appState)
        {
            var request = await CreateSyncRequest(appState);
            return await _Synchronize(request, appState);

        }


        //public static Enough.Async.AsyncLock _SynchronizeLock = new Enough.Async.AsyncLock();


        //protected async Task<ISyncInstance> _Synchronize(ISyncInstance s)
        //{
        //    using (var release = await _SynchronizeLock.LockAsync())
        //    {
        //        return await _UnsafeSynchronize(s);
        //    }
        //}


        private async Task<ISyncInstance> _Synchronize(ISyncInstance s, IGSAppState appState = null)
        {
            bool handlePull = false;
            IPhotoDownloadRequest[] downloadRequests = s.PhotoDownloadRequests;

            if (!s.PullReq.IsEmpty)
            {
                var pullResp = await s.Pull();
                if (pullResp != null
                    && pullResp.StatusCode == GSStatusCode.OK
                    && pullResp.Streams != null
                    && pullResp.Streams.Count > 0)
                {
                    Handler.AttachAggregates(pullResp);
                    handlePull = true;
                    if (s.PushReq.IsEmpty)
                    {
                        Handler.Handle(new Pull(s));
                        if (appState != null)
                            downloadRequests = appState.PhotoDownloads.Values.Select(x => RequestF.CreatePhotoDownloadRequest(x)).ToArray();
                    }
                }

                if (pullResp == null || pullResp.StatusCode != GSStatusCode.OK)
                {
                    // this suggests access token has expired, next sync will use new one
                    // should handle this better, but this will do for now 
                    if (pullResp.StatusCode == GSStatusCode.AUTHENTICATION_REQUIRED)
                    {
                        AuthService.ClearAuthorization();
                    }
                    s.Status = SyncStatus.PULL_ERROR;
                    return s;
                }
            }


            if (!s.PushReq.IsEmpty)
            {
                if (handlePull)
                {
                    try
                    {
                        s.Merge();
                    }
                    catch
                    {
                        if (Debugger.IsAttached) { Debugger.Break(); };
                        s.Status = SyncStatus.MERGE_ERROR;
                        return s;
                    }
                }

                var pushResp = await s.Push();
                if (pushResp != null && pushResp.StatusCode == GSStatusCode.OK)
                {
                    if (handlePull)
                    {
                        Handler.Handle(new Pull(s));
                        if (appState != null)
                            downloadRequests = appState.PhotoDownloads.Values.Select(x => RequestF.CreatePhotoDownloadRequest(x)).ToArray();
                    }
                    Handler.Handle(new Push(s));
                }

                if (pushResp == null || (pushResp.StatusCode != GSStatusCode.OK
                    && pushResp.StatusCode != GSStatusCode.VERSION_TOO_LOW))
                {
                    s.Status = SyncStatus.PUSH_ERROR;
                    return s;
                }
            }

            if (s.PhotoUploadRequests.Length > 0 && appState != null)
            {
                var responses = await s.UploadPhotos();
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK)
                    .Select(x => new CompletePhotoUpload(x) { AncestorId = appState.User.Id }).ToArray();
                if (successes.Length > 0)
                    Handler.Handle(new StreamSegment(appState.Id, successes));

                foreach (var resp in responses)
                {
                    if (resp.StatusCode != GSStatusCode.OK)
                    {
                        s.Status = SyncStatus.PHOTOUPLOAD_ERROR;
                        return s;
                    }
                }
            }

            if (downloadRequests.Length > 0 && appState != null)
            {
                var responses = await s.DownloadPhotos(downloadRequests);
                var successes = responses.Where(x => x.StatusCode == GSStatusCode.OK)
                    .Select(x => new CompletePhotoDownload(x.Photo)).ToArray();

                if (successes.Length > 0)
                    Handler.Handle(new StreamSegment(appState.Id, successes));

                foreach (var resp in responses)
                {
                    if (resp.StatusCode != GSStatusCode.OK)
                    {
                        s.Status = SyncStatus.PHOTODOWNLOAD_ERROR;
                        return s;
                    }
                }
            }
            s.Status = SyncStatus.OK;


            return s;
        }




        // Special push for creating user
        //
        private async Task<ISyncInstance> PushCreateUser(SyncHead head)
        {
            var s = new SyncInstance
            (
               RequestF.CreatePullRequest(null),
               RequestF.CreatePushRequest(head),
               new IPhotoUploadRequest[0],
               null
            );
            //await Prev(currentSynchronize);
            return await this._Synchronize(s);
        }


        private IDisposable AutoSyncSubscription = Disposable.Empty;

        public IDisposable SubscribeForAutoSync(IGSAppState appState)
        {
            if (appState == null || !IsAutoSyncEnabled)
                return Disposable.Empty;

            this.Log().Info("Subscribed for autosync");

            AutoSyncSubscription.Dispose();
            AutoSyncSubscription = this.MessageBus
                .Listen<IEvent>()
                .OfType<EventBase>()
                .Where(e =>
                {
                    if (appState.User == null || IsAutosyncTemporarilyDisabled)
                        return false;
                    if (e.AggregateId == appState.User.Id
                       || e.AncestorId == appState.User.Id
                       || e.EntityId == appState.User.Id
                       || e.StreamAncestorId == appState.User.Id
                       || e.StreamEntityId == appState.User.Id)
                    {
                        return true;

                    }
                    return false;
                })
                .Throttle(new TimeSpan(0, 0, ThrottleInterval))
                .Select(_ => appState)
                .Subscribe(x =>
                {
                    this.Log().Info("AutoSync started");
                    var r = SyncAll(x);
                });


            return AutoSyncSubscription;
        }




        // Run multiple synchronization sequences, until everything is pushed
        // or until a maximum amount of sequences is reached
        //
        public async Task<Tuple<AllSyncResult, GSStatusCode?>> SyncAll(IGSAppState appState, int maxRounds = 20)
        {

            using (await AcquireLock())
            {

                //await Prev(prevSyncAll);
                var debugId = Guid.NewGuid().ToString();

                this.Log().Info("SyncAll starting, debugId: " + debugId);

                int counter = 0;
                ISyncInstance R = null;
                GSStatusCode? nullResponseCode = null;

                while (counter < maxRounds)
                {
                    R = await Synchronize(appState);
                    counter++;

                    if (R.Status != SyncStatus.OK)
                    {
                        return Tuple.Create(AllSyncResult.Error, nullResponseCode);
                    }

                    // TODO: check if there is more stuff to pull

                    if (R.PushReq.IsEmpty)
                    {
                        return Tuple.Create(AllSyncResult.AllSynced, nullResponseCode);
                    }
                }

                this.Log().Info("SyncAll finished, debugId: " + debugId);

                return Tuple.Create(AllSyncResult.SomeLeft, nullResponseCode);
            }
        }




        //
        // ONLY FOR TESTING
        // not necessarily thread safe, do not call from app code
        //
        //public static async Task<Tuple<AllSyncResult, GSStatusCode?>> PushAll(this IGSAppViewModel app, int maxRounds = 20)
        //{
        //    int counter = 0;
        //    ISyncInstance R = null;
        //    GSStatusCode? nullResponseCode = null;
        //    while (counter < maxRounds)
        //    {
        //        R = await app.Push();
        //        counter++;
        //        if (R == null) // there is nothing to do
        //        {
        //            return Tuple.Create(AllSyncResult.AllSynced, nullResponseCode);
        //        }
        //        else if (R.PushReq.IsEmpty)
        //        {
        //            return Tuple.Create(AllSyncResult.AllSynced, nullResponseCode);
        //        }
        //        else if (R.PushResp.StatusCode != GSStatusCode.OK)
        //        {
        //            nullResponseCode = R.PushResp.StatusCode;
        //            return Tuple.Create(AllSyncResult.Error, nullResponseCode);

        //        }
        //    }

        //    return Tuple.Create(AllSyncResult.SomeLeft, nullResponseCode);
        //}





    }

    public sealed class NonAutoSyncingSynchronizer : SynchronizerService
    {
        public NonAutoSyncingSynchronizer(
          IDispatchCommands handler,
          IRequestFactory requestF,
          IMessageBus messageBus,
          IUserService authService
          )
            : base(handler, requestF, messageBus, authService, false)
        {
        }
    }

    public sealed class AutoSyncingSynchronizer : SynchronizerService
    {
        public AutoSyncingSynchronizer(
          IDispatchCommands handler,
          IRequestFactory requestF,
          IMessageBus messageBus,
          IUserService authService
          )
            : base(handler, requestF, messageBus, authService, true)
        {
        }
    }


}
