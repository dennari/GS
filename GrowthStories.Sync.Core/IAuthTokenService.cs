using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    public interface IAuthTokenService
    {
        Task<IAuthTokenResponse> GetAuthToken(string username, string password);
    }
}
