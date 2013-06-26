using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Growthstories.Sync
{


    public interface ISyncPullRequest : ISyncRequest
    {
        ICollection<ISyncEventStream> Streams { get; }
    }



}