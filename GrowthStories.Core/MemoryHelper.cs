using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Core
{
    public class MemoryHelper
    {

        public static void CollectGarbageForTesting()
        {
            //GC.Collect(2, GCCollectionMode.Forced, true); // useful for testing
        }

    }
}
