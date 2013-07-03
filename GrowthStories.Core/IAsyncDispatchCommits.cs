using EventStore.Dispatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Core
{
    public interface IAsyncDispatchCommits : IDispatchCommits
    {
        /// <summary>
        ///  Dispatches the _EVENTS_ that were registered to be handled with async handlers in previous SYNCHRONOUS commits
        /// </summary>
        /// <returns></returns>
        Task DispatchAsync();

    }
}
