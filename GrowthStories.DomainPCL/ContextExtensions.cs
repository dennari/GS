using Ninject.Activation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain
{
    public static class ContextExtensions
    {
        public static T GetParameter<T>(this IContext c, string pName) where T : class
        {
            try
            {
                foreach (var p in c.Parameters)
                {
                    if (p.Name == pName)
                    {
                        return (T)p.GetValue(c, null);
                    }
                }
            }
            catch (Exception) { }

            return null;
        }
    }
}
