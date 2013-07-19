using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;


namespace Growthstories.Sync
{
    public interface IResponseFactory
    {
        ISyncPullResponse CreatePullResponse(string reponse);

        ISyncPushResponse CreatePushResponse(string response);

    }

}
