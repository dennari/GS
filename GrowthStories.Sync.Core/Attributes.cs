using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Sync
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DTOObjectAttribute : Attribute
    {
        public readonly DTOType Type;
        public DTOObjectAttribute(DTOType type)
        {
            this.Type = type;
        }
    }

}
