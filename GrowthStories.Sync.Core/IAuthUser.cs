using CommonDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IAuthUser : IAuthTokenResponse, IMemento
    {
        string Username { get; }
        string Password { get; }
        string Email { get; }
        Guid GardenId { get; }
    }
}
