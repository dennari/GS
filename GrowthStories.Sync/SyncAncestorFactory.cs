using CommonDomain;
using Growthstories.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public class SyncUserService : IUserService
    {
        private IAuthUser _Current;

        public SyncUserService()
        {

        }


        public IAuthUser CurrentUser
        {
            get
            {
                return _Current;
            }
            set
            {
                _Current = value;
            }
        }
    }
}
