namespace Growthstories.Sync
{
    using System;

    /// <summary>
    /// Indicates the most recent synced version of a stram
    /// </summary>
    public class SyncHead : IEquatable<SyncHead>
    {
        /// <summary>
        /// Initializes a new instance of the StreamHead class.
        /// </summary>
        /// <param name="streamId">The value which uniquely identifies the stream where the last snapshot exceeds the allowed threshold.</param>
        /// <param name="headRevision">The value which indicates the revision, length, or number of events committed to the stream.</param>
        /// <param name="snapshotRevision">The value which indicates the revision at which the last snapshot was taken.</param>
        public SyncHead(Guid streamId, int syncedRevision)
        {
            this.StreamId = streamId;
            this.SyncedRevision = syncedRevision;
        }



        /// <summary>
        /// Gets the value which uniquely identifies the stream where the last snapshot exceeds the allowed threshold.
        /// </summary>
        public Guid StreamId { get; private set; }

        /// <summary>
        /// Gets the value which indicates the revision, length, or number of events committed to the stream.
        /// </summary>
        public int SyncedRevision { get; private set; }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>If the two objects are equal, returns true; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SyncHead);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.StreamId.GetHashCode();
        }

        public bool Equals(SyncHead other)
        {
            if (other == null)
                return false;
            return StreamId == other.StreamId;
        }

        public static bool operator ==(SyncHead person1, SyncHead person2)
        {
            if ((object)person1 == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(SyncHead person1, SyncHead person2)
        {
            if (person1 == null || person2 == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }
    }
}