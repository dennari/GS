using Growthstories.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Growthstories.Domain.Entities
{

    public abstract class Handler<TEvent>
    {

        public const string HANDLER_METHOD = "When";

        public IDictionary<Type, MethodInfo> Handlers { get; private set; }

        public Handler()
        {
            RegisterHandlers();
        }


        private void RegisterHandlers()
        {
            var mesg = typeof(TEvent).GetTypeInfo();

            Handlers = this.GetType().GetTypeInfo().DeclaredMethods
                 .Where(m => m.Name == HANDLER_METHOD)
                 .Where(m => m.GetParameters().Length == 1)
                 .Where(m => mesg.IsAssignableFrom(m.GetParameters().First().ParameterType.GetTypeInfo()))
                 .ToDictionary(m => m.GetParameters().First().ParameterType, m => m);
        }

    }
}
