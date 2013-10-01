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
        void Rebase(ISyncEventStream remoteStream);

        //void CommitPullChanges(Guid commitId);

        Commit[] Commits { get; }

        IEnumerable<IEventDTO> Translate(ITranslateEvents translator);

        void AddRemote(IEvent e);

        ICollection<IEvent> UncommittedRemoteEvents { get; }

        void CommitRemoteChanges(Guid commitId);

        void Add(IEvent e, bool setVersion = false);
    }

    public interface ISyncEventStreamDTO
    {
        string Type { get; }
        int SinceVersion { get; }
        Guid StreamId { get; }
        Guid? StreamAncestorId { get; }
    }


}
