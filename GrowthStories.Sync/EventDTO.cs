using Growthstories.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Growthstories.Sync
{
    public interface IEventDTO : IEquatable<IEventDTO>
    {
        string type { get; set; }
        Guid targetEntityId { get; set; }
        Guid targetAncestorId { get; set; }
        Guid parentId { get; set; }
        Guid parentAncestorId { get; set; }
        int incId { get; set; }
        DateTimeOffset createdOn { get; set; }
        Guid guid { get; set; }


    }

    public class EventDTO : IEventDTO
    {
        public static readonly string[] Required = new string[] {
            "targetEntityId",
            "targetEntityId",
            "guid",
            "incId",
            "type"
        };

        public static readonly Func<JObject, EventDTO>[] Creators = new Func<JObject, EventDTO>[] { 
            PlantAddedDTO.CreateIfMatches,
            SetPropertyDTO.CreateIfMatches
        };

        public static string[] GetRequired()
        {
            return Required;
        }

        public static bool Matches(JObject o)
        {
            return Required.All(x => o[x] != null);
        }

        public EventDTO()
        {
            createdOn = DateTimeOffset.UtcNow;
        }
        public EventDTO(IEvent @event)
            : this()
        {
            targetEntityId = @event.EntityId;
            incId = @event.EntityVersion;
            guid = @event.EventId;

        }

        public string type { get; set; }
        public Guid targetEntityId { get; set; }
        public Guid targetAncestorId { get; set; }
        public Guid parentId { get; set; }
        public Guid parentAncestorId { get; set; }
        public int incId { get; set; }
        public DateTimeOffset createdOn { get; set; }
        public Guid guid { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>If the two objects are equal, returns true; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as IEventDTO);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.guid.GetHashCode();
        }

        public bool Equals(IEventDTO other)
        {
            if (other == null)
                return false;
            return this.guid == other.guid;
        }

        public static bool operator ==(EventDTO person1, EventDTO person2)
        {
            if ((object)person1 == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(EventDTO person1, EventDTO person2)
        {
            if (person1 == null || person2 == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }


    }
}