using EventStore;
using Growthstories.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface ISyncEventStream : IEventStream, IEquatable<ISyncEventStream>
    {
        //void Rebase(ISyncEventStream remoteStream);

        //void CommitPullChanges(Guid commitId);

        GSCommit[] Commits { get; }

        IEnumerable<IEventDTO> Translate(ITranslateEvents translator);

        void AddRemote(IEvent e);

        ICollection<IEvent> UncommittedRemoteEvents { get; }

        void CommitRemoteChanges(Guid commitId);

        /// <summary>
        /// Commits the changes to durable storage.
        bool MarkCommitsSynchronized(ISyncPushResponse pushResp = null);

        void Add(IEvent e, bool setVersion = false);

        SyncStreamType Type { get; }
        long SyncStamp { get; }
    }

    public interface ISyncEventStreamDTO
    {
        string Type { get; }
        long SyncStamp { get; }
        Guid StreamId { get; }
        Guid? StreamAncestorId { get; }
    }


}
