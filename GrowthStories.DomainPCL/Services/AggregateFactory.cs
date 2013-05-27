using CommonDomain;
using CommonDomain.Persistence;
using System;
using System.Reflection;
using Growthstories.Core;
using Ninject;

namespace Growthstories.Domain.Services
{
    public class AggregateFactory : IConstructAggregates
    {
        //private readonly IKernel Container;

        public AggregateFactory()
        {
            //Container = container;
        }

        public IAggregate Build<T>(Guid id, IMemento snapshot) where T : IAggregate
        {

            //if (snapshot == null)
            //{
            //    Type stateType;
            //    TypeInfo paramType;
            //    TypeInfo candidate;
            //    TypeInfo mementoType = typeof(IMemento).GetTypeInfo();
            //    TypeInfo currentType = typeof(T).GetTypeInfo();
            //    foreach (var constructor in currentType.DeclaredConstructors)
            //    {
            //        if (constructor.IsPublic)
            //        {
            //            foreach (var parameter in constructor.GetParameters())
            //            {
            //                if (parameter.Position > 0)
            //                    stateType = null;
            //                break;
            //                paramType = parameter.ParameterType.GetTypeInfo();
            //                if (paramType.IsAssignableFrom(mementoType))
            //                {
            //                    stateType = paramType.AsType();
            //                }
            //            }
            //            if (stateType != null)
            //                break;

            //        }

            //    }
            //    if (stateType == null)
            //    {
            //        throw new InvalidOperationException(string.Format("No suitable constructor found in {0}", currentType));
            //    }


            //    //snapshot = Activator.CreateInstance(stateType);
            //}
            var instance = Activator.CreateInstance<T>();
            ((IApplyState)instance).ApplyState(snapshot);
            return instance;
        }

    }
}
