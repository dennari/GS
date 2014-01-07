
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Growthstories.Sync
{
    public interface IJsonFactory
    {
        string Serialize(object o);

        T Deserialize<T>(string i);
    }


}
