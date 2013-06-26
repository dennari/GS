using CommonDomain;
using Growthstories.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Growthstories.Sync
{


    public interface ISyncPushResponse : ISyncResponse
    {

        Guid ClientDatabaseId { get; }
        Guid PushId { get; }
        bool AlreadyExecuted { get; }

        /**
         * Last command that was executed
         * 
         * value is zero (0) if no commands were executed
         */
        Guid LastExecuted { get; }

        /**
         * Status code
         */
        int StatusCode { get; }

        String StatusDesc { get; }

        //public Map<String, Long> guids = new HashMap<String, Long>();

    }



}
