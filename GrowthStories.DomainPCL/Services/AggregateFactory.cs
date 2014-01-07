using CommonDomain;
using CommonDomain.Persistence;
using System;
using System.Reflection;
using Growthstories.Core;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Growthstories.Domain.Services
{





    public class AggregateFactory : IAggregateFactory
    {
        private IEventFactory eFactory;

        private IDictionary<Type, Func<IGSAggregate>> constructorCache = new Dictionary<Type, Func<IGSAggregate>>();

        public AggregateFactory(IEventFactory eFactory)
        {
            this.eFactory = eFactory;
        }

        public IGSAggregate Build(Type type)
        {
            Func<IGSAggregate> constructor = null;
            if (!constructorCache.TryGetValue(type, out constructor))
            {
                constructor = CreateDefaultConstructor<IGSAggregate>(type);
                lock (constructorCache)
                {
                    constructorCache[type] = constructor;
                }
            }

            var instance = constructor();
            instance.SetEventFactory(eFactory);
            return instance;
        }

        public T Build<T>() where T : IGSAggregate, new()
        {
            var instance = new T();
            instance.SetEventFactory(eFactory);
            return instance;
        }

        public T Build<T>(IMemento state) where T : IGSAggregate, new()
        {
            if (state.Id == default(Guid) || state.Version == 0)
            {
                throw new ArgumentException();
            }
            var instance = Build<T>();
            instance.ApplyState(state);
            return instance;
        }

        public Func<T> CreateDefaultConstructor<T>(Type type)
        {
            //ValidationUtils.ArgumentNotNull(type, "type");

            // avoid error from expressions compiler because of abstract class
            //if (type.IsAbstract())
            //    return () => (T)Activator.CreateInstance(type);

            try
            {
                Type resultType = typeof(T);

                Expression expression = Expression.New(type);

                expression = EnsureCastExpression(expression, resultType);

                LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T>), expression);

                Func<T> compiled = (Func<T>)lambdaExpression.Compile();
                return compiled;
            }
            catch
            {
                // an error can be thrown if constructor is not valid on Win8
                // will have INVOCATION_FLAGS_NON_W8P_FX_API invocation flag
                return () => (T)Activator.CreateInstance(type);
            }
        }

        private Expression EnsureCastExpression(Expression expression, Type targetType)
        {
            Type expressionType = expression.Type;

            // check if a cast or conversion is required
            if (expressionType == targetType || (!expressionType.IsValueType() && targetType.IsAssignableFrom(expressionType)))
                return expression;

            return Expression.Convert(expression, targetType);
        }


    }
}
