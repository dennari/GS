using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IAuthTokenResponse : ISyncResponse
    {
        string AccessToken { get; }
        int ExpiresIn { get; }
        string RefreshToken { get; }
    }
}
