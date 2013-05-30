using Growthstories.Core;
using System;

namespace Growthstories.Domain.Entities
{
    public class CommentState : AggregateState<CommentCreated>
    {
        public CommentState() { }
        public CommentState(Guid id, int version, bool Public) : base(id, version, Public) { }
    }
}
