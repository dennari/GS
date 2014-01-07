namespace Growthstories.Sync
{
    using EventStore.Logging;
    using System;
    using System.Threading.Tasks;
    using System.Linq;



    public sealed class SyncInstance : ISyncInstance
    {

        public SyncStatus Status { get; set; }
        public GSStatusCode Code { get; set; }

        public ISyncPullRequest PullReq { get; private set; }
        public ISyncPushRequest PushReq { get; private set; }

        public ISyncPushResponse PushResp { get; private set; }
        public ISyncPullResponse PullResp { get; private set; }

        public IPhotoUploadRequest[] PhotoUploadRequests { get; private set; }
        public IPhotoDownloadRequest[] PhotoDownloadRequests { get; private set; }

        private static ILog Logger = LogFactory.BuildLogger(typeof(SyncInstance));


        public SyncInstance(SyncStatus s)
        {
            Status = s;
        }

        public SyncInstance(
            ISyncPullRequest aPullReq,
            ISyncPushRequest aPushReq,
            IPhotoUploadRequest[] PhotoUploadRequests = null,
            IPhotoDownloadRequest[] PhotoDownloadRequests = null)
        {
            if (aPullReq == null || aPushReq == null)
                throw new ArgumentNullException("Both pullrequest and pushrequest need to be provided for SyncInstance.");

            this.PullReq = aPullReq;
            this.PushReq = aPushReq;
            this.PhotoUploadRequests = PhotoUploadRequests ?? new IPhotoUploadRequest[] { };
            this.PhotoDownloadRequests = PhotoDownloadRequests ?? new IPhotoDownloadRequest[] { };
            this.Status = SyncStatus.OK;
        }

        public async Task<ISyncPullResponse> Pull()
        {
            PullResp = await PullReq.GetResponse();
            return PullResp;

        }

        public async Task<ISyncPushResponse> Push()
        {
            PushResp = await PushReq.GetResponse();
            return PushResp;
        }

        public Task<IPhotoUploadResponse[]> UploadPhotos()
        {
            if (PhotoUploadRequests == null)
                return null;


            return Task.WhenAll(PhotoUploadRequests.Select(x => x.GetResponse()));

        }

        public Task<IPhotoDownloadResponse[]> DownloadPhotos(IPhotoDownloadRequest[] overrideReq = null)
        {
            if (PhotoDownloadRequests == null && overrideReq == null)
                return null;


            return Task.WhenAll((overrideReq ?? PhotoDownloadRequests).Select(x => x.GetResponse()));

        }

        public int Merge()
        {

            if (PullResp == null || PullReq == null || PullReq.IsEmpty)
                return 0;

            // let us first see if there are conflicting concurrent changes
            // as this will couple the push and pull
            //Tuple<IAggregateMessages, IAggregateMessages>[] conflictingStreams = null;

            var q = from pull in PullResp.Streams
                    join push in PushReq.Streams on pull.AggregateId equals push.AggregateId
                    where (pull.Count > 0 && push.Count > 0)
                    select Tuple.Create(push, pull);

            var conflicts = q.ToArray();

            foreach (var c in conflicts)
            {
                c.Item1.MergeOutgoing(c.Item2);
                c.Item2.MergeIncoming(c.Item1);
            }

            return conflicts.Length;
        }



    }






}