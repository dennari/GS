using CommonDomain;
using CommonDomain.Persistence;
using System;

namespace Growthstories.Core
{
    public static class RepositoryExtensions
    {
        public static void Save(this IRepository repository, IAggregate aggregate)
        {
            repository.Save(aggregate, Guid.NewGuid(), (headers) => {
                //headers.Add()
            });
        }
    }
}
