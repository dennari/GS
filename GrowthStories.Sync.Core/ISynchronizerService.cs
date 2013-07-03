

using System.Collections.Generic;
using System.Threading.Tasks;
namespace Growthstories.Sync
{
    public interface ISynchronizerService
    {
        Task<IList<ISyncRequest>> Synchronize();

        IEnumerable<ISyncEventStream> Pending();
    }
}
