using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using Growthstories.Core;
using Growthstories.Domain.Entities;
using Microsoft.CSharp.RuntimeBinder;
//using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Growthstories.Domain.Messaging
{
    public class CommandHandler : ICommandHandler<ICommand>
    {

        readonly IRepository _repository;

        public CommandHandler(IRepository store)
            : base()
        {
            _repository = store;
        }


        void DoIt(Func<IAggregate> action)
        {

            _repository.Save(action(), Guid.NewGuid());

        }



        public void When(CreateUser c)
        {

            DoIt(() =>
            {
                var u = _repository.GetById<User>(c.EntityId);
                u.Create(c.EntityId);
                return u;
            });
        }

        public void When(CreatePlant c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<Plant>(c.EntityId);
                u.Create(c);
                return u;
            });
        }

        public void When(MarkPlantPublic c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<Plant>(c.EntityId);
                u.Public = true;
                return u;
            });
        }

        public void When(MarkPlantPrivate c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<Plant>(c.EntityId);
                u.Public = false;
                return u;
            });
        }


        public void When(CreateGarden c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<Garden>(c.EntityId);
                u.Create(c.EntityId);
                return u;
            });
        }

        public void When(AddPlant c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<Plant>(c.PlantId);
                u.Create(c);
                return u;
            });

            DoIt(() =>
            {
                var a = _repository.GetById<Garden>(c.EntityId);
                a.AddPlant(c.PlantId, c.PlantName);
                return a;
            });
        }

        public void When(MarkGardenPublic c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<Garden>(c.EntityId);
                u.Public = true;
                return u;
            });
        }

        public void When(CreatePlantAction c)
        {
            DoIt(() =>
            {
                var u = _repository.GetById<PlantAction>(c.EntityId);
                u.Create(c.EntityId);
                return u;
            });
        }

        public void Handle(ICommand e)
        {
            //MethodInfo h;
            //if (!Handlers.TryGetValue(e.GetType(), out h))
            //{
            //    var s = string.Format("Failed to locate {0}.When({1})", this.GetType().Name, e.GetType().Name);
            //    throw new InvalidOperationException(s);
            //}
            //try
            //{
            //    h.Invoke(this, new object[] { e });
            //}
            //catch (TargetInvocationException ex)
            //{
            //    throw ex.InnerException;
            //}

            try
            {
                ((dynamic)this).When((dynamic)e);
            }
            catch (RuntimeBinderException ee)
            {
                throw;
            }
        }

    }
}
