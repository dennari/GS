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
        private readonly IPhotoHandler FileOpener;


        public RequestFactory(
            ITranslateEvents translator,
            ITransportEvents transporter,
            IPhotoHandler fileOpener,
            IJsonFactory jFactory,
            IPersistSyncStreams syncPersistence)
        {
            this.jFactory = jFactory;
            this.Translator = translator;
            this.Transporter = transporter;
            this.FileOpener = fileOpener;
            this.SyncPersistence = syncPersistence;
        }



        public ISyncPushRequest CreatePushRequest(SyncHead currentSyncHead)
        {

            var nextEvent = GetNextPushEvent(currentSyncHead);

            ICollection<IStreamSegment> streams = new IStreamSegment[] { };
            if (nextEvent.Item1 != null)
                streams = new[] { new StreamSegment(nextEvent.Item1.AggregateId, new[] { nextEvent.Item1 }) };

            var req = new HttpPushRequest(jFactory)
            {
                SyncHead = nextEvent.Item2,
                Streams = streams,
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator,
                Transporter = Transporter
            };


            return req;
        }

        public ISyncPushRequest CreatePushRequest(IEnumerable<IStreamSegment> streams, SyncHead syncHead)
        {
            var streamsC = streams.ToArray();

            var req = new HttpPushRequest(jFactory)
            {
                SyncHead = syncHead,
                Streams = streamsC,
                ClientDatabaseId = Guid.NewGuid(),
                Translator = Translator,
                Transporter = Transporter
            };


            return req;
        }


        public Tuple<IEvent, SyncHead> GetNextPushEvent(SyncHead currentSyncHead)
        {

            if (currentSyncHead == null)
                throw new ArgumentNullException("currentSyncHead needs to be given");
            // finds unsynchronized commits (pull commits  are discarded) with sequence GREATER THAN commitNum
            var nextCommit = SyncPersistence.GetUnsynchronizedCommits(currentSyncHead.GlobalCommitSequence)
                .Where(x =>
                {
                    return x.StreamId != GSAppState.GSAppId;
                }).FirstOrDefault();

            IEvent nextEvent = null;
            SyncHead nextSyncHead = currentSyncHead;
            if (nextCommit != null)
            {
                nextEvent = (IEvent)nextCommit.Events[currentSyncHead.EventIndex].Body;
                nextSyncHead = GetNexSyncHead(currentSyncHead, nextCommit);
            }
            return Tuple.Create(nextEvent, nextSyncHead);


        }


        private SyncHead GetNexSyncHead(SyncHead currentSyncHead, GSCommit nextCommit)
        {
            if (currentSyncHead.NumEvents <= 1) // this means we are dealing with a fresh commit
            {
                if (nextCommit.Events.Count > 1)
                {
                    return new SyncHead(
                        currentSyncHead.GlobalCommitSequence,
                        nextCommit.Events.Count,
                        1
                    );

                }
                else
                {
                    return new SyncHead(nextCommit.GlobalCommitSequence, 0, 0);
                }
            }
            else
            {
                if (currentSyncHead.EventIndex < nextCommit.Events.Count - 1)
                {
                    return new SyncHead(
                        currentSyncHead.GlobalCommitSequence,
                        nextCommit.Events.Count,
                        currentSyncHead.EventIndex + 1
                    );
                }
                else
                {
                    return new SyncHead(nextCommit.GlobalCommitSequence, 0, 0);
                }
            }
        }


        private IEnumerable<IStreamSegment> GetPushStreams(int globalSequence)
        {
            var validOnes = SyncPersistence.GetUnsynchronizedCommits(globalSequence)
                .Where(x =>
                {
                    return x.StreamId != GSAppState.GSAppId;
                })
                .SelectMany(x => x.ActualEvents())
                .OfType<IDomainEvent>()
                .Where(x => IsTranslatable(x))
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
                SyncHead = GetNexSyncHead(new SyncHead(0, 0, 0), (GSCommit)commit),
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

        public IPhotoUploadRequest CreatePhotoUploadRequest(Tuple<Photo, Guid> photo)
        {

            return new PhotoUploadRequest(photo.Item1, photo.Item2, jFactory, Transporter, FileOpener);

        }

        public IPhotoDownloadRequest CreatePhotoDownloadRequest(Tuple<Photo, Guid> photo)
        {

            return new PhotoDownloadRequest(photo.Item1, photo.Item2, jFactory, Transporter, FileOpener);

        }


    }


}
