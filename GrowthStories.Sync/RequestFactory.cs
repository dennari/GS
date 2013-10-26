using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using EventStore.Logging;
using Growthstories.Core;
using System.Net.Http.Headers;
using System.Net;
using Growthstories.Domain.Messaging;
using Growthstories.Domain.Entities;
using EventStore.Persistence;
using EventStore;

namespace Growthstories.Sync
{

    public class RequestFactory : IRequestFactory
    {
        private readonly IJsonFactory jFactory;
        private readonly ITranslateEvents Translator;
        private readonly IPersistSyncStreams SyncPersistence;
        private static ILog Logger = LogFactory.BuildLogger(typeof(RequestFactory));
        private readonly ITransportEvents Transporter;
        private readonly IFileOpener FileOpener;

        public RequestFactory(
            ITranslateEvents translator,
            ITransportEvents transporter,
            IFileOpener fileOpener,
            IJsonFactory jFactory,
            IPersistSyncStreams syncPersistence)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
            this.Transporter = transporter;
            this.FileOpener = fileOpener;
            this.SyncPersistence = syncPersistence;
        }



        public ISyncPushRequest CreatePushRequest(int globalSequence)
        {

            return CreatePushRequest(GetPushStreams(globalSequence), globalSequence);
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<IStreamSegment> streams, int globalSequence)
        {
            var streamsC = streams.ToArray();

            var req = new HttpPushRequest(jFactory)
            {
                GlobalCommitSequence = globalSequence,
                Streams = streamsC,
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator,
                Transporter = Transporter
            };


            return req;
        }

        private IEnumerable<IStreamSegment> GetPushStreams(int globalSequence)
        {
            var validOnes = SyncPersistence.GetUnsynchronizedCommits(globalSequence)
                .Where(x => x.StreamId != GSAppState.GSAppId)
                .SelectMany(x => x.ActualEvents())
                .OfType<IDomainEvent>()
                .Where(x => IsTranslatable(x))
                .ToList()
                .GroupBy(x => x.AggregateId);


            foreach (var stream in validOnes)
            {

                yield return new StreamSegment(stream);
            }
        }

        private bool IsTranslatable(IDomainEvent x)
        {

            return x.GetDTOType() != null;
        }

        public ISyncPushRequest CreateUserSyncRequest(Guid userId)
        {
            //var streamsC = 

            //var ee = Translator.Out(streamsC).ToArray();

            var commit = SyncPersistence.GetFrom(userId, 0, 1).First();

            var stream = new StreamSegment(userId);
            stream.Add(commit.ActualEvents().First());

            var req = new HttpPushRequest(jFactory)
            {
                Streams = new[] { stream },
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator,
                Transporter = Transporter
            };

            return req;
        }






        public ISyncPullRequest CreatePullRequest(ICollection<PullStream> streams)
        {
            //var streamsC = streams.ToArray();
            return new HttpPullRequest(jFactory)
            {
                Streams = streams,
                Translator = Translator,
                Transporter = Transporter
            };
            //return req;
        }

        public IPhotoUploadRequest CreatePhotoUploadRequest(Photo photo)
        {

            return new PhotoUploadRequest(photo, jFactory, Transporter, FileOpener);

        }

        public IPhotoDownloadRequest CreatePhotoDownloadRequest(Photo photo)
        {

            return new PhotoDownloadRequest(photo, jFactory, Transporter, FileOpener);

        }


    }


}
